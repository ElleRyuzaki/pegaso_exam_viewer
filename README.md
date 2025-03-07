# Pegaso Exam Viewer

Un tool per visualizzare gli esami sostenuti sulla piattaforma Multiversity ed esportare domande e risposte.

## 🔹 Come funziona?
Il tool autentica l'utente utilizzando le credenziali fornite, recupera il token di autorizzazione e utilizza un client secret, estrapolato tramite reverse engineering, per effettuare richieste alle API di Pegaso.

## 📌 Prerequisiti
Per utilizzare il tool, assicurati di avere installato il .NET SDK. Puoi scaricarlo qui:
[Scarica .NET SDK](https://dotnet.microsoft.com/it-it/download/dotnet/thank-you/sdk-8.0.406-windows-x64-installer)

## 🚀 Avvio del Tool
Per iniziare, segui questi passi:
- **Opzione 1:** Scarica l'ultima release dal repository, decomprimi l'archivio e avvia il file `.exe`.
- **Opzione 2:** Clona questa repository e avviala manualmente.

## 🛠️ Utilizzo
L'uso del tool è semplicissimo:
1. Inserisci username e password.
2. Il sistema autentica l'utente e ottiene il token di autorizzazione.
3. Viene effettuata una richiesta alle API di Pegaso utilizzando il client secret.
4. Visualizza la lista degli esami sostenuti.
5. Digita il numero corrispondente all'esame per vedere domande e risposte.
6. Esporta i dati con un semplice copia e incolla.

