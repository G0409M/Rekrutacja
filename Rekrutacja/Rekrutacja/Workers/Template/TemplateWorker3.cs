using Soneta.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soneta.Kadry;
using Soneta.KadryPlace;
using Soneta.Types;
using Rekrutacja.Workers.Template;
using static Soneta.Ksiega.ZestawienieKS;

//Rejetracja Workera - Pierwszy TypeOf określa jakiego typu ma być wyświetlany Worker, Drugi parametr wskazuje na jakim Typie obiektów będzie wyświetlany Worker
[assembly: Worker(typeof(TemplateWorker3), typeof(Pracownicy))]
namespace Rekrutacja.Workers.Template
{
    public class TemplateWorker3
    {
        //Aby parametry działały prawidłowo dziedziczymy po klasie ContextBase
        public class TemplateWorker3Parametry : ContextBase
        {
            [Caption("A")]
            public double A { get; set; }

            [Caption("B")]
            public double B { get; set; }

            [Caption("Data obliczeń")]
            public Date DataObliczen { get; set; }
            [Caption("Operacja")]
            public string Operacja { get; set; }
            public TemplateWorker3Parametry(Context context) : base(context)
            {
                this.A = 0;
                this.B = 0;
                this.Operacja = "+";
                this.DataObliczen = Date.Today;
            }
        }
        //Obiekt Context jest to pudełko które przechowuje Typy danych, aktualnie załadowane w aplikacji
        //Atrybut Context pobiera z "Contextu" obiekty które aktualnie widzimy na ekranie
        [Context]
        public Context Cx { get; set; }
        //Pobieramy z Contextu parametry, jeżeli nie ma w Context Parametrów mechanizm sam utworzy nowy obiekt oraz wyświetli jego formatkę
        [Context]
        public TemplateWorker3Parametry Parametry { get; set; }
        //Atrybut Action - Wywołuje nam metodę która znajduje się poniżej
        [Action("Kalkulator2",
           Description = "Prosty kalkulator obliczania pola figury ",
           Priority = 10,
           Mode = ActionMode.ReadOnlySession,
           Icon = ActionIcon.Accept,
           Target = ActionTarget.ToolbarWithText)]

        public void WykonajAkcje()
        {
            //Włączenie Debug, aby działał należy wygenerować DLL w trybie DEBUG
            DebuggerSession.MarkLineAsBreakPoint();
            //Pobieranie danych z Contextu
            Pracownik[] pracownicy = null;
            if (this.Cx.Contains(typeof(Pracownik[])))
            {
                pracownicy = (Pracownik[])this.Cx[typeof(Pracownik[])];
            }

            //Modyfikacja danych
            //Aby modyfikować dane musimy mieć otwartą sesję, która nie jest read only
            using (Session nowaSesja = this.Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))
            {
                //Otwieramy Transaction aby można było edytować obiekt z sesji
                using (ITransaction trans = nowaSesja.Logout(true))
                {
                    foreach (Pracownik pracownik in pracownicy)
                    {
                        //Pobieramy obiekt z Nowo utworzonej sesji
                        var pracownikZSesja = nowaSesja.Get(pracownik);
                        //Features - są to pola rozszerzające obiekty w bazie danych, dzięki czemu nie jestesmy ogarniczeni to kolumn jakie zostały utworzone przez producenta
                        pracownikZSesja.Features["DataObliczen"] = this.Parametry.DataObliczen;
                        pracownikZSesja.Features["Wynik"] = Oblicz(this.Parametry.A, this.Parametry.B, this.Parametry.Operacja);
                        //Zatwierdzamy zmiany wykonane w sesji
                        trans.CommitUI();
                    }

                }
                //Zapisujemy zmiany
                nowaSesja.Save();
            }
        }
        private double Oblicz(double a, double b, string operacja)
        {
            switch (operacja)
            {
                case "+":
                    return a + b;
                case "-":
                    return a - b;
                case "*":
                    return a * b;
                case "/":
                    return b != 0 ? a / b : 0;
                default:
                    throw new InvalidOperationException("Nieprawidłowa operacja");
            }
        }
    }
}