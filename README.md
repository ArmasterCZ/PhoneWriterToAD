# TelefonyDoAD
Slouží k rozřazení čísel z .SCV seznamu a následnému zápisu do Active Directory. Provedené změny, případně nalezené chyby, zapíše do těla informačního emailu a odešle na email uvedený v app.config.


Skládá se z třídy pro načtení a základní rozřazení dat z CSV. (CsvLoader.cs)
Třídy pro rozřazení jednotlivých čísel do atributů z AD. (numberRedistribution.cs)
Třídy pro načtení uživatelů a zápis čísel v AD. (AdConnection.cs)
Třídy pro odeslání emailu + načtení dat z app.config.  (Email.cs)
Tříd pro seskupení načtených dat (AdUser) & finálních změn (TelephoneUser).
