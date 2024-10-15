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
[assembly: Worker(typeof(TemplateWorker), typeof(Pracownicy))]//stałe
namespace Rekrutacja.Workers.Template
{
    public class TemplateWorker
    {
        //Aby parametry działały prawidłowo dziedziczymy po klasie ContextBase
        public class TemplateWorkerParametry : ContextBase//stałe
        {
            [Caption("A")]
            public double A { get; set; }

            [Caption("B")]
            public double B { get; set; }

            [Caption("Data obliczeń")]//stałe
            public Date DataObliczen { get; set; }//stałe
            [Caption("Operacja")]
            public string Operacja { get; set; }
            public TemplateWorkerParametry(Context context) : base(context)//stałe
            {
                this.A = 0;
                this.B = 0;
                this.Operacja = "+";
                this.DataObliczen = Date.Today;//stałe
            }
        }
        //Obiekt Context jest to pudełko które przechowuje Typy danych, aktualnie załadowane w aplikacji
        //Atrybut Context pobiera z "Contextu" obiekty które aktualnie widzimy na ekranie
        [Context]
        public Context Cx { get; set; }//stałe
        //Pobieramy z Contextu parametry, jeżeli nie ma w Context Parametrów mechanizm sam utworzy nowy obiekt oraz wyświetli jego formatkę
        [Context]
        public TemplateWorkerParametry Parametry { get; set; }//stałe
        //Atrybut Action - Wywołuje nam metodę która znajduje się poniżej
        [Action("Kalkulator",
           Description = "Prosty kalkulator ",
           Priority = 10,
           Mode = ActionMode.ReadOnlySession,
           Icon = ActionIcon.Accept,
           Target = ActionTarget.ToolbarWithText)]//stałe

        public void WykonajAkcje()//stałe
        {
            //Włączenie Debug, aby działał należy wygenerować DLL w trybie DEBUG
            DebuggerSession.MarkLineAsBreakPoint();//stałe
            //Pobieranie danych z Contextu
            Pracownik[] pracownicy = null;//stałe
            if (this.Cx.Contains(typeof(Pracownik[])))//stałe
            {
                pracownicy = (Pracownik[])this.Cx[typeof(Pracownik[])];//stałe
            }

            //Modyfikacja danych
            //Aby modyfikować dane musimy mieć otwartą sesję, która nie jest read only
            using (Session nowaSesja = this.Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))//stałe
            {
                //Otwieramy Transaction aby można było edytować obiekt z sesji
                using (ITransaction trans = nowaSesja.Logout(true))//stałe
                {
                    foreach(Pracownik pracownik in pracownicy)
                    {
                        //Pobieramy obiekt z Nowo utworzonej sesji
                        var pracownikZSesja = nowaSesja.Get(pracownik);//stałe
                         //Features - są to pola rozszerzające obiekty w bazie danych, dzięki czemu nie jestesmy ogarniczeni to kolumn jakie zostały utworzone przez producenta
                        pracownikZSesja.Features["DataObliczen"] = this.Parametry.DataObliczen;//stałe
                        pracownikZSesja.Features["Wynik"] = Oblicz(this.Parametry.A, this.Parametry.B, this.Parametry.Operacja);
                        //Zatwierdzamy zmiany wykonane w sesji
                        trans.CommitUI();//stałe
                    }
                   
                }
                //Zapisujemy zmiany
                nowaSesja.Save();//stałe
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