# Forumas

# Forumo API

## Apžvalga

Forumo API yra RESTful žiniatinklio paslauga, sukurta naudojant ASP.NET Core 8. Api leidžia vartotojams kurti, skaityti, atnaujinti ir trinti temas, įrašus ir komentarus diskusijų forume. Duomenys saugomi PostgreSQL duomenų bazėje.

## Funkcijos

- **Temos**: Kurti ir valdyti diskusijų temas.
- **Įrašai**: Pridėti įrašus po konkrečiomis temomis.
- **Komentarai**: Leisti vartotojams komentuoti įrašus.
- **CRUD operacijos**: Realizuotos bazinės CRUD operacijos.
- Autentifikacija ir autorizacija: Naudotojai turi registruotis ir autentifikuotis, kad galėtų kurti, redaguoti ar trinti temas, įrašus ir komentarus.
- Pagalba ir valdymas: Administratoriui suteikiama galimybė tvarkyti turinį.

## Naudojamos technologijos

- **Rėmas**: ASP.NET Core 8
- **Duomenų bazė**: PostgreSQL
- **Entity Framework Core**: Duomenų prieigai ir duomenų bazės valdymui
- **Fluent Validation**: Įeinančių duomenų patikrinimui
- JSON Web Token (JWT): Naudojamas autentifikacijai ir autorizacijai

## Duomenų bazės schema

Duomenų bazė susideda iš trijų pagrindinių lentelių:

1. **Temos**
    - **Id**: Unikalus identifikatorius kiekvienai temai (pagrindinis raktas)
    - **Pavadinimas**: Temos pavadinimas
    - **Aprašymas**: Trumpas temos aprašymas
    - **Sukurtas**: Laiko žymė, kada tema buvo sukurta
    - **YraIštrinta**: Boolean, nurodantis, ar tema buvo ištrinta

2. **Įrašai**
    - **Id**: Unikalus identifikatorius kiekvienam įrašui (pagrindinis raktas)
    - **Pavadinimas**: Įrašo pavadinimas
    - **Turinys**: Įrašo turinys
    - **Sukurtas**: Laiko žymė, kada įrašas buvo sukurtas
    - **YraIštrinta**: Boolean, nurodantis, ar įrašas buvo ištrintas
    - **TemaId**: Užsienio raktas, jungiantis prie Temų lentelės

3. **Komentarai**
    - **Id**: Unikalus identifikatorius kiekvienam komentarui (pagrindinis raktas)
    - **Turinys**: Komentaro tekstas
    - **Sukurtas**: Laiko žymė, kada komentaras buvo sukurtas
    - **YraIštrinta**: Boolean, nurodantis, ar komentaras buvo ištrintas
    - **ĮrašoId**: Užsienio raktas, jungiantis prie Įrašų lentelės

## Pradžia / paleidimas
Atsidarius Visual Studio atidaromas .sln projektas, vienu lygiu nusileidžiama į /forum sub katalogą, atsidarius terminalą paleidžiama komandą "dotnet ef database update" esant Entities direktorijoje terminale iškviečiama "docker-compose up" komandą, galiausiai paspaudus Ctrl+f5 arba "start without debugging" arba esant /forum sub kataloge į terminalą parašius "dotnet run" turėtų pasileisti API projekto dalis, pasiekiamia localhost:5052.
Esant forum-front/forum direktorijoje terminale iškviečiamas "npm install", toliau "npm start"

### Reikalavimai

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) įdiegtas ir veikiantis
- Node.js įdiegtas ir veikiantis
- NPM įdiegtas ir veikiantis
