# Specifiche di Conformità Legale per Software di Rilevazione Presenze e Orario di Lavoro (Diritto Svizzero)

Questo documento raccoglie tutti i requisiti legali, tecnici, funzionali e di protezione dati necessari per sviluppare, implementare o configurare un software di timbrature e gestione dell'orario di lavoro conforme alla legislazione della Confederazione Svizzera.

---

## 1. Quadro Normativo di Riferimento

Lo sviluppo e l'adozione di software per la timbratura oraria in Svizzera devono rispettare congiuntamente le seguenti normative:

* **Legge federale sul lavoro (LL - RS 822.11)**, in particolare:
  * *Art. 46*: Obbligo del datore di lavoro di tenere registri orari a disposizione delle autorità di vigilanza.
* **Ordinanza 1 concernente la legge sul lavoro (OLL 1 - RS 822.111)**:
  * *Art. 73*: Registrazione ordinaria dell'orario di lavoro.
  * *Art. 73a*: Rinuncia alla registrazione dell'orario di lavoro (esenzione).
  * *Art. 73b*: Registrazione semplificata dell'orario di lavoro.
* **Ordinanza 3 concernente la legge sul lavoro (OLL 3 - RS 822.113)**:
  * *Art. 26*: Divieto di sistemi di controllo e sorveglianza del comportamento dei lavoratori.
* **Nuova Legge federale sulla protezione dei dati (nLPD - RS 235.1)**:
  * Principi di trasparenza, proporzionalità, minimizzazione e trattamento di dati sensibili/biometrici.
* **Codice delle obbligazioni (CO - RS 220)**:
  * *Art. 321c*: Trattamento e compensazione del lavoro straordinario/supplementare (*Überstunden*).
* **Linee guida SECO (Segreteria di Stato dell'economia)** e prassi degli Ispettorati cantonali del lavoro.

---

## 2. I Tre Modelli di Rilevazione Oraria (OLL 1)

Il software deve supportare l'assegnazione a livello di singolo collaboratore di una delle tre modalità legali previste:

### 2.1. Registrazione Ordinaria (Art. 73 OLL 1)
È la regola generale applicabile a tutti i lavoratori che non soddisfano i requisiti di rinuncia o semplificazione.
* **Dati obbligatori:**
  * Orario preciso di inizio e termine della prestazione lavorativa (ora e minuto).
  * Collocazione temporale e durata delle pause uguali o superiori a 30 minuti (e di quelle in cui è concesso lasciare il posto di lavoro).
  * Totale delle ore lavorate su base giornaliera e settimanale.
  * Giorni di riposo settimanale goduti o compensativi.
  * Registrazione distinta del lavoro notturno (23:00 – 06:00) e domenicale (sabato 23:00 – domenica 23:00).
  * Monitoraggio e calcolo del lavoro straordinario (*Überzeit*).

### 2.2. Registrazione Semplificata (Art. 73b OLL 1)
Applicabile previa convenzione collettiva di lavoro (CCL) o accordo tra direzione e rappresentanza dei lavoratori (o accordo individuale per aziende con meno di 50 dipendenti), per collaboratori con un grado rilevante di autonomia nella gestione dell'orario.
* **Requisiti di tracciamento:**
  * Richiede solo l'indicazione del **numero totale di ore lavorate al giorno**.
  * Non è obbligatorio registrare gli orari precisi di ingresso, uscita e pausa.
  * **Requisito di fallback:** Il lavoratore deve avere la facoltà di richiedere in qualsiasi momento il ritorno alla registrazione ordinaria.

### 2.3. Rinuncia alla Registrazione (Art. 73a OLL 1)
Riservata a quadri dirigenti e specialisti con stipendio annuo lordo superiore a **120'000 CHF** (inclusi bonus) che godono di un'autonomia di almeno il **50%** nella pianificazione dell'orario.
* **Requisiti software:**
  * Profilo contrattuale contrassegnato come "Esente da timbratura".
  * Tracciamento della validità dell'accordo formale scritto (soggetto a rinnovo/revisione annuale).

---

## 3. Limiti di Legge e Validazioni Algoritmiche (Guardrail)

Il software deve integrare sistemi di calcolo, warning e blocco basati sui parametri del diritto del lavoro:

| Regola di Legge | Parametro / Limite | Logica di Controllo Software |
|---|---|---|
| **Durata massima settimanale** (Art. 9 LL) | **45 ore**: Industria, uffici, tecnici, grande distribuzione.<br>**50 ore**: Artigianato, edilizia, ristorazione, logistica. | Warning al superamento della soglia settimanale; categorizzazione automatica in lavoro straordinario (*Überzeit*). |
| **Arco giornaliero massimo** (Art. 10 LL) | Massimo **14 ore** comprensive di pause e ore straordinarie. | Alert se tra il primo timestamp di inizio e l'ultimo di fine intercorrono più di 14 ore. |
| **Riposo giornaliero consecutivo** (Art. 15a LL) | Almeno **11 ore consecutive** di riposo tra due turni (riducibile a 9 ore eccezionalmente). | Blocco o warning se un dipendente timbra l'entrata prima che siano trascorse 11 ore dall'ultima uscita del turno precedente. |
| **Pause minime obbligatorie** (Art. 15 LL) | - Da > 5.5 ore di lavoro: **15 minuti**<br>- Da > 7 ore di lavoro: **30 minuti**<br>- Da > 9 ore di lavoro: **60 minuti** | Segnalazione nel cartellino se le pause minime non sono state effettuate o non raggiungono la durata minima di legge. |
| **Fasce orarie speciali** (Art. 16, 17, 18 LL) | Notturno ordinario: 23:00 - 06:00.<br>Domenica: Sabato 23:00 - Domenica 23:00. | Differenziazione automatica dei contatori orari per applicare supplementi legali (es. maggiorazione oraria o in tempo del 10%-25%). |

---

## 4. Requisiti di Audit, Conservazione e Trasparenza

Secondo le direttive SECO e le prassi degli ispettorati del lavoro cantonali:

1. **Conservazione quinquennale (Art. 73 cpv. 2 OLL 1):**
   * Tutti i dati di timbratura, le correzioni manuali e i riepiloghi mensili devono essere archiviati e rimanere accessibili per almeno **5 anni**.
2. **Accesso per il lavoratore:**
   * Ciascun dipendente deve poter accedere in autonomia al proprio cartellino orario, verificare il saldo ore, le ferie residue e le ore cumulative.
3. **Audit Trail Immodificabile:**
   * Nessuna voce può essere cancellata o sovrascritta in modo opaco.
   * Se un amministratore o un dipendente rettifica una timbratura omessa o errata, il database deve conservare:
     * Valore originale e valore rettificato.
     * Identificativo (User ID) di chi ha effettuato la modifica.
     * Timestamp dell'operazione.
     * Motivazione obbligatoria della correzione.
4. **Ispezioni del lavoro:**
   * Funzione di export immediata (PDF, CSV, Excel) conforme alle richieste degli ispettori cantonali, con distinzione trasparente di orari, straordinari e pause.

---

## 5. Protezione dei Dati Personali (nLPD) e Sorveglianza (OLL 3)

### 5.1. Divieto di Sorveglianza Continua (Art. 26 OLL 3)
* È categoricamente vietato utilizzare il software per monitorare costantemente il comportamento o la posizione del dipendente durante la giornata.
* Nessun tracciamento GPS in background o monitoraggio continuo dell'attività (es. keylogger, screenshot automatici).

### 5.2. Timbratura Mobile e Geolocalizzazione
* **Geofencing / Punto GPS:** La geolocalizzazione è lecita unicamente come **evento puntuale (timestamp geografico)** al momento del click di entrata/uscita, utile a certificare la presenza in un cantiere o presso una filiale.
* L'applicazione mobile deve richiedere il permesso di posizione solo "durante l'uso dell'app" e memorizzare unicamente le coordinate dell'evento di timbratura.
* I dipendenti devono ricevere una chiara informativa privacy preventiva.

### 5.3. Trattamento dei Dati Biometrici
* I dati biometrici (impronte digitali, riconoscimento facciale) costituiscono dati personali degni di particolare protezione ex nLPD.
* In caso di hardware con lettore biometrico:
  * È vivamente sconsigliato salvare immagini biometriche raw nel database centrale.
  * Utilizzare modelli basati su template crittografici memorizzati localmente sul badge (*Match-on-Card*) o cifrati irreversibilmente.
  * Offrire sempre un metodo alternativo non biometrico (es. PIN, badge RFID).

---

## 6. Checklist Tecnica per l'Architettura Software

- [ ] **Data Model:** Supporto a orari pianificati vs. orari effettivi, separazione di *Überstunden* e *Überzeit*.
- [ ] **Configuratore Contratti:** Gestione dei profili dipendente (Ordinario Art. 73, Semplificato Art. 73b, Esente Art. 73a).
- [ ] **Motore di Regole:** Calcolo automatico di pause minime, riposo minimo tra turni (11h) e soglie settimanali (45h/50h).
- [ ] **Audit Log:** Tabella dedicata `time_entry_audit_logs` con `entry_id`, `changed_by`, `old_value`, `new_value`, `reason`, `timestamp`.
- [ ] **Retention Policy:** Archiviazione automatica e prevenzione cancellazione per almeno 60 mesi (5 anni).
- [ ] **Privacy by Design:** Nessuna geolocalizzazione continua; acquisizione coordinate solo su evento `Clock-In`/`Clock-Out`.
- [ ] **Reporting:** Generazione esportazioni standard per verifiche ispettive cantonali / SECO.