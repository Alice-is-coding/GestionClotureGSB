using System;
using System.Data;
using MySql.Data.MySqlClient;
using MyTools;

namespace GestionClotureGSB
{
    class Program
    {
        // Declarations.
        private DateTime actualDate = DateTime.Today;
        private BDConnection maCnx = MyTools.BDConnection.GetBDConnection("localhost", "gsb_frais", "root", "root");
        public static Program program = new Program();

        /// <summary>
        /// Campagne de validation des fiches de frais,
        /// Si nous nous trouvons entre le 1 et le 10 du mois courant, 
        /// cloture des fiches du mois N-1 en passant leur état de "CR" à "CL".
        /// </summary>
        public void CloturerFicheFrais()
        {
            // Si on se trouve actuellement entre le 1er et le 10 :
            if (MyTools.DateManagement.Between(1, 10))
            {
                // Mise à jour de l'état de la fiche ("CR" à "CL") (les fiches du mois précédent son forcément à l'état CR donc inutile de tester ce champ).
                maCnx.ReqUpdate("UPDATE fichefrais SET idetat = 'CL' WHERE mois = '" + actualDate.Year + MyTools.DateManagement.GetPreviousMonth() + "'");
            }
        }

        /// <summary>
        /// A partir du 20ème jour du mois,
        /// mise à jour de l'état de la fiche de frais ("MP" à "RB"),
        /// pour informer que celle-ci a bien été remboursée.
        /// </summary>
        public void RembourserFicheFrais()
        {
            // A partir du 20ème jour du mois : 
            if (MyTools.DateManagement.Between(20, 31))
            {
                // Mise à jour de l'état de la fiche ("MP" à "RB").
                maCnx.ReqUpdate("UPDATE fichefrais SET idetat = 'RB' WHERE mois = '" + actualDate.Year + MyTools.DateManagement.GetPreviousMonth() + "' AND idetat = 'MP'");
            }
        }
        static void Main(string[] args)
        {
            // Cloture des fiches de frais.
            program.CloturerFicheFrais();
            // Remboursement des fiches de frais.
            program.RembourserFicheFrais();
   
            Console.ReadLine();
        }
    }
}
