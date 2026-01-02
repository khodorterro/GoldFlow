using System.Security.Cryptography.Pkcs;
using DataAccessLayer;
namespace BussinnessLayer
{
    public class People
    {

        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode = enMode.AddNew;

        public PersonDTO PDTO
        {
            get { return (new PersonDTO(this.ID, this.Name, this.address, this.phone,this.email)); }
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string email { get; set; }


        public People(PersonDTO PDTO, enMode cMode = enMode.AddNew)

        {
            this.ID = PDTO.Id;
            this.Name = PDTO.Name;
            this.phone = PDTO.Phonenumber;
            this.address = PDTO.Address;
            this.email = PDTO.Email;

            Mode = cMode;
        }

        private bool _AddNewPerson()
        {
            this.ID = PeopleData.AddPerson(PDTO);

            return (this.ID != -1);
        }

        private bool _UpdatePerson()
        {
            return PeopleData.UpdatePerson(ID,PDTO);
        }

        public static People Find(int ID)
        {

            PersonDTO PDTO = PeopleData.GetPersonByID(ID);
            if (PDTO != null)
            {
                return new People(PDTO, enMode.Update);
            }
            else
            {
                return null;
            }
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewPerson())
                    {

                        Mode = enMode.Update;
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case enMode.Update:

                    return _UpdatePerson();

            }

            return false;
        }

        public static bool DeletePerson(int ID)
        {
            return PeopleData.DeletePerson(ID);
        }


    }
}
