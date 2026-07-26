using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum MerkmalFahrzeug : long
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        Antrieb = 40143874L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Basisausführung = 99999999850L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Baureihe = 40140418L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Baureihenverbund = 99999999950L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        EBezeichnung = 40140802L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        ELebenszyklus = 99999999858L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        ElektrischeReichweite = 99999999854L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Getriebe = 40141186L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Grundtyp = 40139652L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Hybridkennzeichen = 68771233666L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Karosserie = 40146178L,
        [EditorBrowsable(EditorBrowsableState.Never)]
        Länderausführung = 40146562L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Lebenszyklus = 99999999856L,
        [EditorBrowsable(EditorBrowsableState.Never)]
        Leittyp = 99999999905L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Lenkung = 40141954L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Marke = 40144642L,
        [EditorBrowsable(EditorBrowsableState.Never)]
        Montageland = 99999999851L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Produktart = 40140034L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Produktlinie = 40039947266L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Sicherheitsfahrzeug = 40145410L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Türen = 40144258L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Typschlüssel = 40139650L,
        [EditorBrowsable(EditorBrowsableState.Never)]
        Verkaufsbezeichnung = 40143490L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Sportausfuehrung = 99999999846L
    }
}
