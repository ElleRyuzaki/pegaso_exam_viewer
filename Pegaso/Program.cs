using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string baseUrl = "https://app-api.pegaso.multiversity.click";
        string loginEndpoint = "/oauth/token";
        string examListEndpoint = "/api/exam-online/sost";
        string examResultEndpointTemplate = "/api/exam-online/test/result/{0}";

        Console.WriteLine("=== Pegaso Exam Viewer ===");
        Console.WriteLine("Un tool per visualizzare gli esami sostenuti sulla piattaforma Multiversity e permetterti di esportare domande e risposte.");
        Console.WriteLine("by ElleRyuzaki");
        Console.Write("Inserisci il tuo username: ");
        string username = Console.ReadLine();

        Console.Write("Inserisci la tua password: ");
        string password = ReadPassword();

        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);

                // Aggiunta degli header richiesti
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                client.DefaultRequestHeaders.Add("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 12; sdk_gphone64_x86_64 Build/SE1A.211212.001.B1)");

                // Step 1: Autenticazione per ottenere il token
                var loginData = new
                {
                    username,
                    password,
                    grant_type = "password",
                    client_id = "2",
                    client_secret = "NMLI8oHBpsLfwaYyn0uosWlSVUJtqHse4ZZC4WxM",
                    scope = "*"
                };

                var jsonLoginData = JsonSerializer.Serialize(loginData);
                var content = new StringContent(jsonLoginData, Encoding.UTF8, "application/json");

                Console.WriteLine("Autenticazione in corso...");
                HttpResponseMessage loginResponse = await client.PostAsync(loginEndpoint, content);

                if (!loginResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Errore di autenticazione: {loginResponse.StatusCode}");
                    return;
                }

                // Recupera il token di accesso
                string loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(loginResponseContent);

                if (authResponse == null || string.IsNullOrEmpty(authResponse.AccessToken))
                {
                    Console.WriteLine("Token non ricevuto.");
                    return;
                }

                string accessToken = authResponse.AccessToken;
                Console.WriteLine("Token ottenuto con successo!");

                // Imposta il token per le richieste successive
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Step 2: Ottieni la lista degli esami sostenuti
                Console.WriteLine("\nRecupero della lista degli esami sostenuti...");
                HttpResponseMessage examListResponse = await client.GetAsync(examListEndpoint);

                if (!examListResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Errore nel recupero della lista degli esami: {examListResponse.StatusCode}");
                    return;
                }

                string examListJson = await examListResponse.Content.ReadAsStringAsync();
                var examList = JsonSerializer.Deserialize<ExamListResponse>(examListJson);

                if (examList?.Data == null || examList.Data.Count == 0)
                {
                    Console.WriteLine("Nessun esame trovato.");
                    return;
                }

                // Mostra la lista degli esami e chiedi all'utente di selezionarne uno
                var selectedExam = ChooseExam(examList.Data);
                if (selectedExam == null)
                {
                    Console.WriteLine("Nessun esame selezionato.");
                    return;
                }

                // Step 3: Recupera le informazioni dettagliate sull'esame selezionato
                string examResultEndpoint = string.Format(examResultEndpointTemplate, selectedExam.Id);
                Console.WriteLine($"\nRecupero dettagli dell'esame: {selectedExam.NameExam}...");

                HttpResponseMessage examResponse = await client.GetAsync(examResultEndpoint);

                if (!examResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Errore nel recupero dati dell'esame: {examResponse.StatusCode}");
                    return;
                }

                string examData = await examResponse.Content.ReadAsStringAsync();
                var examResult = JsonSerializer.Deserialize<ExamResult>(examData);

                if (examResult?.Data?.Responses != null)
                {
                    Console.WriteLine("\n=============================================");
                    Console.WriteLine($"ESAME: {selectedExam.NameExam}");
                    Console.WriteLine($"DATA: {selectedExam.ExamDate}");
                    Console.WriteLine($"RISULTATO: {selectedExam.Points}/30 - {selectedExam.StatusName}");
                    Console.WriteLine("=============================================\n");

                    int questionNumber = 1;
                    foreach (var response in examResult.Data.Responses)
                    {
                        Console.WriteLine($"Domanda {questionNumber}: {response.Question}");
                        Console.WriteLine($"Risposta: {response.Answer}");
                        Console.WriteLine($"Punti: {response.Point}\n");
                        Console.WriteLine("-------------------------------------------\n");
                        questionNumber++;
                    }

                    Console.WriteLine($"Punteggio finale: {selectedExam.Points}/30");
                    Console.WriteLine($"Stato: {selectedExam.StatusName}");
                }
                else
                {
                    Console.WriteLine("Nessuna risposta trovata per questo esame.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errore: " + ex.Message);
        }

        Console.WriteLine("\nPremi un tasto per uscire...");
        Console.ReadKey();
    }

    private static string ReadPassword()
    {
        StringBuilder password = new StringBuilder();
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b"); // Cancella l'ultimo carattere *
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password.ToString();
    }

    // Funzione per mostrare la lista degli esami e permettere all'utente di sceglierne uno
    private static ExamData ChooseExam(List<ExamData> exams)
    {
        Console.WriteLine("\nEsami disponibili:");
        Console.WriteLine("----------------------------------------------------------");
        Console.WriteLine("ID | Data       | Nome Corso                   | Risultato");
        Console.WriteLine("----------------------------------------------------------");

        for (int i = 0; i < exams.Count; i++)
        {
            var exam = exams[i];
            string date = exam.ExamDate ?? "N/A";
            string points = exam.Points.ToString() ?? "N/A";
            string status = exam.StatusName ?? "N/A";

            Console.WriteLine($"{i + 1,-3}| {date,-11}| {exam.NameExam,-28}| {points}/30 - {status}");
        }

        Console.WriteLine("----------------------------------------------------------");

        while (true)
        {
            Console.Write("\nInserisci il numero dell'esame che vuoi visualizzare (o 0 per uscire): ");
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == 0)
                {
                    return null;
                }

                if (choice >= 1 && choice <= exams.Count)
                {
                    return exams[choice - 1];
                }
            }

            Console.WriteLine("Scelta non valida. Riprova.");
        }
    }
}

// Classe per deserializzare la risposta di autenticazione
class AuthResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}

// Classe per deserializzare la lista degli esami
class ExamListResponse
{
    [JsonPropertyName("data")]
    public List<ExamData> Data { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }
}

class ExamData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("exam_id")]
    public int ExamId { get; set; }

    [JsonPropertyName("name_exam")]
    public string NameExam { get; set; }

    [JsonPropertyName("exam_date")]
    public string ExamDate { get; set; }

    [JsonPropertyName("points")]
    public int Points { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("status_name")]
    public string StatusName { get; set; }
}

// Classe per deserializzare i risultati dell'esame
class ExamResult
{
    [JsonPropertyName("data")]
    public ExamResultData Data { get; set; }
}

class ExamResultData
{
    [JsonPropertyName("responses")]
    public List<Response> Responses { get; set; }
}

class Response
{
    [JsonPropertyName("question")]
    public string Question { get; set; }

    [JsonPropertyName("answer")]
    public string Answer { get; set; }

    [JsonPropertyName("point")]
    public int Point { get; set; }
}