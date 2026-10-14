# Funktsiooni uurimine

See on veebirakendus, mis aitab uurida matemaatilisi funktsioone. Kasutaja sisestab funktsiooni valemi ja rakendus arvutab selle peamised omadused ning näitab graafikut. Projekt on loodud õppetöö jaoks.

## Mida rakendus teeb?

- Leiab funktsiooni määramispiirkonna.
- Leiab funktsiooni nullkohad.
- Arvutab tuletise.
- Leiab kriitilised punktid.
- Leiab kohalikud maksimum- ja miinimumpunktid.
- Joonistab funktsiooni graafiku.
- Võimaldab funktsioone salvestada, vaadata, muuta ja kustutada.

## Kasutatud tehnoloogiad

- C# ja ASP.NET Core MVC
- .NET 10
- MathNet.Symbolics
- Entity Framework Core ja SQLite
- HTML, CSS ja JavaScript
- Chart.js

## Rakenduse käivitamine

Rakenduse käivitamiseks tuleb paigaldada [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

1. Laadi projekt alla või klooni GitHubi repositoorium.
2. Ava terminal ja mine projekti kausta:

   ```bash
   cd FunktsiooniUurimine
   ```

3. Käivita rakendus:

   ```bash
   dotnet run
   ```

4. Ava veebibrauseris terminalis näidatud kohalik aadress.

Andmebaas kasutab SQLite'i. Rakenduse käivitamisel rakendatakse andmebaasi migratsioonid automaatselt.

## Kuidas rakendust kasutada?

1. Ava rakendus veebibrauseris.
2. Vali uue funktsiooni lisamine.
3. Sisesta valem muutujaga `x`. Näiteks: `x^3 - 6*x + 3`.
4. Salvesta funktsioon ja vaata tulemusi.
5. Tulemuste lehel näed arvutusi, samm-sammulist selgitust ja funktsiooni graafikut.

## Martin Rossakov
