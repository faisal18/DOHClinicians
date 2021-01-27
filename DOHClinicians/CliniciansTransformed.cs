using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOHClinicians
{
    class CliniciansTransformed
    {
        public string License { get; set; }
        public string name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FacilityLicense { get; set; }
        public string FacilityName { get; set; }
        public string area { get; set; }
        public string ActiveFrom { get; set; }
        public string ActiveTo { get; set; }
        public string isactive { get; set; }
        public string source { get; set; }
        public string SpecialtyID1 { get; set; }
        public string Specialty { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string SpecialtyID2 { get; set; }
        public string SpecialtyID3 { get; set; }
        public string type { get; set; }
        public string OldLicense { get; set; }

        //EXTRA Fields
        public string SpecialtyFieldID { get; set; }
        public string SpecialtyField { get; set; }
        public string major { get; set; }
        public string profession { get; set; }
        public string HAAD_Category { get; set; }
        public string CurrentStatus { get; set; }

    }
}
