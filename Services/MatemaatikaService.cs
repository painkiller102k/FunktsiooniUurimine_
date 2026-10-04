using System.Globalization;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;
using FunctionModel = FunktsiooniUurimine.Models.FunktsiooniUurimine;

namespace FunktsiooniUurimine.Services;

public class MatemaatikaService
{
    private const double Algus = -10;
    private const double Lopp = 10;
    private const double Samm = 0.01;
    private const double NulliTapsus = 0.00000001;

    public bool TryAnalyze(FunctionModel uurimine)
    {
        if (string.IsNullOrWhiteSpace(uurimine.Valem))
        {
            return false;
        }

        try
        {
            Expr avaldis = Expr.Parse(uurimine.Valem);
            Func<double, double> funktsioon = avaldis.Compile("x");
            Expr tuletiseAvaldis = avaldis.Differentiate("x");
            Func<double, double> tuletis = tuletiseAvaldis.Compile("x");

            uurimine.Maaramispiirkond = LeiaMaaramispiirkond(funktsioon);
            uurimine.Nullkohad = LeiaNullkohad(funktsioon);
            uurimine.Tuletis = tuletiseAvaldis.ToString();
            uurimine.KriitilisedPunktid = LeiaKriitilisedPunktid(funktsioon, tuletis);
            uurimine.Ekstreemumid = LeiaEkstreemumid(funktsioon, tuletis);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public List<double?> ArvutaGraafikuPunktid(
        string valem, double algus, double lopp, double samm, bool arvutaTuletis = false)
    {
        try
        {
            Expr avaldis = Expr.Parse(valem);
            if (arvutaTuletis)
            {
                avaldis = avaldis.Differentiate("x");
            }

            return ArvutaPunktid(avaldis.Compile("x"), algus, lopp, samm);
        }
        catch
        {
            return new List<double?>();
        }
    }

    private string LeiaMaaramispiirkond(Func<double, double> funktsioon)
    {
        var punktid = LooPunktid(funktsioon);
        LeiaPoolused(funktsioon, punktid);
        if (punktid.All(punkt => punkt.Y.HasValue))
        {
            return "(-∞; ∞)";
        }

        var vahemikud = new List<string>();
        int i = 0;

        while (i < punktid.Count)
        {
            if (!punktid[i].Y.HasValue)
            {
                i++;
                continue;
            }

            int algusIndeks = i;
            while (i + 1 < punktid.Count && punktid[i + 1].Y.HasValue)
            {
                i++;
            }

            int loppIndeks = i;
            double? algus = algusIndeks == 0
                ? null
                : punktid[algusIndeks - 1].Poolus
                    ? punktid[algusIndeks - 1].X
                    : LeiaPiir(funktsioon, punktid[algusIndeks - 1].X, punktid[algusIndeks].X, true);
            double? lopp = loppIndeks == punktid.Count - 1
                ? null
                : punktid[loppIndeks + 1].Poolus
                    ? punktid[loppIndeks + 1].X
                    : LeiaPiir(funktsioon, punktid[loppIndeks].X, punktid[loppIndeks + 1].X, false);

            bool algusPoolus = algusIndeks > 0 && punktid[algusIndeks - 1].Poolus;
            bool loppPoolus = loppIndeks < punktid.Count - 1 && punktid[loppIndeks + 1].Poolus;
            string vasakSulund = algus.HasValue && !algusPoolus && VaartusOlemas(funktsioon, Math.Round(algus.GetValueOrDefault(), 3)) ? "[" : "(";
            string paremSulund = lopp.HasValue && !loppPoolus && VaartusOlemas(funktsioon, Math.Round(lopp.GetValueOrDefault(), 3)) ? "]" : ")";
            vahemikud.Add($"{vasakSulund}{(algus.HasValue ? FormaatPiiri(algus.GetValueOrDefault()) : "-∞")}; {(lopp.HasValue ? FormaatPiiri(lopp.GetValueOrDefault()) : "∞")}{paremSulund}");
            i++;
        }

        return vahemikud.Count == 0
            ? "Reaalarvulist määramispiirkonda ei leitud"
            : "Ligikaudu " + string.Join(" ∪ ", vahemikud);
    }

    private string LeiaNullkohad(Func<double, double> funktsioon)
    {
        if (OnNullfunktsioon(funktsioon))
        {
            return "Kõik reaalarvud";
        }

        return FormaatPunktid(LeiaNullid(funktsioon), "Nullkohti vahemikus [-10; 10] ei leitud");
    }

    private string LeiaKriitilisedPunktid(Func<double, double> funktsioon, Func<double, double> tuletis)
    {
        if (OnNullfunktsioon(tuletis))
        {
            return "Kõik punktid";
        }

        return FormaatPunktid(LeiaKriitilisedNullid(funktsioon, tuletis), "Kriitilisi punkte vahemikus [-10; 10] ei leitud");
    }

    private string LeiaEkstreemumid(
        Func<double, double> funktsioon, Func<double, double> tuletis)
    {
        if (OnNullfunktsioon(tuletis))
        {
            return "Funktsioon on konstantne";
        }

        var tulemused = new List<string>();
        foreach (double x in LeiaKriitilisedNullid(funktsioon, tuletis))
        {
            double? vasak = Vaartus(funktsioon, x - 0.001);
            double? parem = Vaartus(funktsioon, x + 0.001);
            double? y = Vaartus(funktsioon, x);

            if (!y.HasValue || (!vasak.HasValue && !parem.HasValue))
            {
                continue;
            }

            if ((!vasak.HasValue || y < vasak) && (!parem.HasValue || y < parem))
            {
                tulemused.Add($"Miinimum: ({Formaat(x)}; {Formaat(y.GetValueOrDefault())})");
            }
            else if ((!vasak.HasValue || y > vasak) && (!parem.HasValue || y > parem))
            {
                tulemused.Add($"Maksimum: ({Formaat(x)}; {Formaat(y.GetValueOrDefault())})");
            }
        }

        return tulemused.Count == 0
            ? "Kohalikke ekstreemume ei leitud"
            : string.Join("; ", tulemused);
    }

    private List<double> LeiaNullid(Func<double, double> funktsioon)
    {
        var punktid = LooPunktid(funktsioon);
        var nullid = new List<double>();

        for (int i = 0; i < punktid.Count; i++)
        {
            if (!punktid[i].Y.HasValue)
            {
                continue;
            }

            double y = punktid[i].Y.GetValueOrDefault();
            if (Math.Abs(y) < NulliTapsus)
            {
                LisaNull(nullid, punktid[i].X);
            }

            if (i == 0 || !punktid[i - 1].Y.HasValue)
            {
                continue;
            }

            double eelmineY = punktid[i - 1].Y.GetValueOrDefault();
            if (Math.Sign(eelmineY) != Math.Sign(y))
            {
                double? nullkoht = LeiaNullBisectioniga(
                    funktsioon, punktid[i - 1].X, punktid[i].X);
                if (nullkoht.HasValue)
                {
                    LisaNull(nullid, nullkoht.Value);
                }
            }

            if (i < punktid.Count - 1 && punktid[i + 1].Y.HasValue &&
                Math.Abs(y) < Math.Abs(eelmineY) &&
                Math.Abs(y) < Math.Abs(punktid[i + 1].Y.GetValueOrDefault()))
            {
                double nullkoht = LeiaVaikseimKoht(
                    funktsioon, punktid[i - 1].X, punktid[i + 1].X);
                double? vaartus = Vaartus(funktsioon, nullkoht);
                if (vaartus.HasValue && Math.Abs(vaartus.Value) < NulliTapsus)
                {
                    LisaNull(nullid, nullkoht);
                }
            }
        }

        return nullid;
    }

    private double? LeiaNullBisectioniga(Func<double, double> funktsioon, double vasak, double parem)
    {
        double? vasakY = Vaartus(funktsioon, vasak);
        if (!vasakY.HasValue)
        {
            return null;
        }

        for (int i = 0; i < 60; i++)
        {
            double kesk = (vasak + parem) / 2;
            double? keskY = Vaartus(funktsioon, kesk);
            if (!keskY.HasValue)
            {
                return null;
            }

            if (Math.Abs(keskY.Value) < NulliTapsus)
            {
                return kesk;
            }

            if (Math.Sign(vasakY.Value) != Math.Sign(keskY.Value))
            {
                parem = kesk;
            }
            else
            {
                vasak = kesk;
                vasakY = keskY;
            }
        }

        double vastus = (vasak + parem) / 2;
        double? kontroll = Vaartus(funktsioon, vastus);
        return kontroll.HasValue && Math.Abs(kontroll.Value) < NulliTapsus ? vastus : null;
    }

    private double LeiaVaikseimKoht(Func<double, double> funktsioon, double vasak, double parem)
    {
        const double kuldneSuhe = 0.61803398875;
        double c = parem - (parem - vasak) * kuldneSuhe;
        double d = vasak + (parem - vasak) * kuldneSuhe;

        for (int i = 0; i < 60; i++)
        {
            double cY = Math.Abs(Vaartus(funktsioon, c) ?? double.PositiveInfinity);
            double dY = Math.Abs(Vaartus(funktsioon, d) ?? double.PositiveInfinity);
            if (cY < dY)
            {
                parem = d;
                d = c;
                c = parem - (parem - vasak) * kuldneSuhe;
            }
            else
            {
                vasak = c;
                c = d;
                d = vasak + (parem - vasak) * kuldneSuhe;
            }
        }

        return (vasak + parem) / 2;
    }

    private double LeiaPiir(Func<double, double> funktsioon, double vasak, double parem, bool kehtivParemal)
    {
        for (int i = 0; i < 40; i++)
        {
            double kesk = (vasak + parem) / 2;
            if (VaartusOlemas(funktsioon, kesk) == kehtivParemal)
            {
                parem = kesk;
            }
            else
            {
                vasak = kesk;
            }
        }

        return kehtivParemal ? parem : vasak;
    }

    private void LeiaPoolused(Func<double, double> funktsioon, List<(double X, double? Y, bool Poolus)> punktid)
    {
        for (int i = 1; i < punktid.Count; i++)
        {
            double? eelmineY = punktid[i - 1].Y;
            double? praeguneY = punktid[i].Y;
            if (!eelmineY.HasValue || !praeguneY.HasValue ||
                Math.Sign(eelmineY.GetValueOrDefault()) == Math.Sign(praeguneY.GetValueOrDefault()) ||
                Math.Abs(eelmineY.GetValueOrDefault()) < 100 || Math.Abs(praeguneY.GetValueOrDefault()) < 100)
            {
                continue;
            }

            double esimenePoordvaartus = 1 / eelmineY.GetValueOrDefault();
            double teinePoordvaartus = 1 / praeguneY.GetValueOrDefault();
            double kandidaat = punktid[i - 1].X - esimenePoordvaartus *
                (punktid[i].X - punktid[i - 1].X) / (teinePoordvaartus - esimenePoordvaartus);
            double? vaartus = Vaartus(funktsioon, kandidaat);

            if (kandidaat > punktid[i - 1].X && kandidaat < punktid[i].X &&
                (!vaartus.HasValue || Math.Abs(vaartus.GetValueOrDefault()) > 1000000))
            {
                punktid.Insert(i, (kandidaat, null, true));
                i++;
            }
        }
    }

    private List<(double X, double? Y, bool Poolus)> LooPunktid(Func<double, double> funktsioon)
    {
        var punktid = new List<(double X, double? Y, bool Poolus)>();
        int kogus = (int)((Lopp - Algus) / Samm);
        for (int i = 0; i <= kogus; i++)
        {
            double x = Math.Round(Algus + i * Samm, 10);
            punktid.Add((x, Vaartus(funktsioon, x), false));
        }

        return punktid;
    }

    private List<double> LeiaKriitilisedNullid(
        Func<double, double> funktsioon, Func<double, double> tuletis)
    {
        var punktid = LeiaNullid(tuletis);
        foreach (var proov in LooPunktid(tuletis))
        {
            if (!proov.Y.HasValue && VaartusOlemas(funktsioon, proov.X))
            {
                LisaNull(punktid, proov.X);
            }
        }

        punktid.Sort();
        return punktid;
    }

    private List<double?> ArvutaPunktid(Func<double, double> funktsioon, double algus, double lopp, double samm)
    {
        var tulemused = new List<double?>();
        for (double x = algus; x <= lopp + samm / 2; x += samm)
        {
            tulemused.Add(Vaartus(funktsioon, Math.Round(x, 10)));
        }

        return tulemused;
    }

    private bool OnNullfunktsioon(Func<double, double> funktsioon)
    {
        double[] kontrollPunktid = { -9.13, -3.27, 0.23, 1.71, 8.19 };
        return kontrollPunktid.All(x =>
        {
            double? y = Vaartus(funktsioon, x);
            return y.HasValue && Math.Abs(y.Value) < NulliTapsus;
        });
    }

    private double? Vaartus(Func<double, double> funktsioon, double x)
    {
        try
        {
            double y = funktsioon(x);
            return double.IsFinite(y) ? y : null;
        }
        catch
        {
            return null;
        }
    }

    private bool VaartusOlemas(Func<double, double> funktsioon, double x) => Vaartus(funktsioon, x).HasValue;

    private void LisaNull(List<double> nullid, double x)
    {
        if (!nullid.Any(nullkoht => Math.Abs(nullkoht - x) < 0.0001))
        {
            nullid.Add(Math.Abs(x) < 0.0005 ? 0 : x);
        }
    }

    private string FormaatPunktid(List<double> punktid, string tyhiTekst) =>
        punktid.Count == 0
            ? tyhiTekst
            : string.Join(", ", punktid.Select(x => $"x = {Formaat(x)}"));

    private string Formaat(double arv) =>
        (Math.Abs(arv) < 0.0005 ? 0 : arv).ToString("0.###", CultureInfo.InvariantCulture);

    private string FormaatPiiri(double arv) =>
        (Math.Abs(arv) < 0.000005 ? 0 : arv).ToString("0.#####", CultureInfo.InvariantCulture);
}