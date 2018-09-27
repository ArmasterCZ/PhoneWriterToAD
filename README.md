# PhoneWriterToAD

`Jméno:` PhoneWriterToAD, 
`Datum:` 2018.06.06, 
`Projekt:` Visual studio *2015-2017*, 
`Framework:` 3.6, 
`Jazyk programu:` Čeština, 
`Jazyk komentářů:` Některé třídy čeština některé angličtina, 
`Popis:` Zápis čísel do AD podle firemní politiky.

`Plný popis:` 
Slouží k rozřazení čísel z .SCV seznamu, následnému zápisu do Active Directory. Provedené změny, případně nalezené chyby, zapíše do těla informačního emailu a odešle na email uvedený v configu.

Skládá z:
- Třídy pro načtení a základní rozřazení dat z CSV. (CsvLoader.cs),
- Třídy pro rozřazení jednotlivých čísel do atributů z AD. (numberRedistribution.cs),
- Třídy pro načtení uživatelů a zápis čísel v AD. (AdConnection.cs),
- Třídy pro odeslání emailu + načtení dat z app.config.  (Email.cs),
- Tříd pro seskupení načtených dat (AdUser) & finálních změn (TelephoneUser).

`Vyuzitá technologie:`
- Linq
- CallBack
- Custom classes
- (poprvé využito) úprava AD skrz System.DirectoryServices

`Špatně využitá technologie:`
- skladování a práce s daty v AdUser.cs
- rozvržení kódu v třídě Program/Main
