using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PhoneBook
{

    class MainModel
    {

        public const string TablePostfix = "_Vasilevsky_Alexander";
        public const string CityTable = "City" + TablePostfix;
        public const string PersonTable = "Person" + TablePostfix;
        public const string ContactTable = "Contact" + TablePostfix;

        public const string SelectAll = "SELECT * FROM @Table";

        public static MainModel Instance = new MainModel();

        public string Connection { get; set; }

        public ICollection<City> GetCities()
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                DataTable data = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(
                    "SELECT [CityId], [CityName], [CityCode], [StateCode] FROM " + CityTable, c);
                adapter.Fill(data);
                Collection<City> result = new Collection<City>();
                foreach (DataRow r in data.AsEnumerable())
                {
                    result.Add(new City(
                        (int) r.ItemArray[0],
                        (string) r.ItemArray[1],
                        (int) r.ItemArray[2],
                        (int) r.ItemArray[3])
                    );
                }
                return result;
            }
        }

        public ICollection<Person> GetPersons()
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                DataTable data = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(
                    "SELECT [PersonId], [PersonName], [BirthDate], [PersonComment], [PersonAddress] FROM " + PersonTable, c);
                adapter.Fill(data);
                Collection<Person> result = new Collection<Person>();
                foreach (DataRow r in data.AsEnumerable())
                {
                    result.Add(new Person(
                        (int)r.ItemArray[0],
                        (string)r.ItemArray[1],
                        (DateTime)r.ItemArray[2],
                        (string)r.ItemArray[3],
                        (string)r.ItemArray[4])
                    );
                }
                return result;
            }
        }

        public ICollection<Contact> GetContacts(
            string namePattern = null,
            string addressPattern = null,
            DateTime? birthDate = null, string phone = null)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                DataTable data = new DataTable();
                SqlCommand cmd = new SqlCommand();
                string command = "SELECT [ContactId], [Phone], [ContactType], "
                + "[CityId], [CityName], [CityCode], [StateCode], "
                + "[PersonId], [PersonName], [BirthDate], [PersonComment], [PersonAddress]"
                + " FROM " + ContactTable + " LEFT JOIN " + PersonTable + " ON [Person] = [PersonId] "
                + " LEFT JOIN " + CityTable + " ON [City] = [CityId]";
                if (!String.IsNullOrWhiteSpace(namePattern)
                    || !String.IsNullOrWhiteSpace(addressPattern)
                    || birthDate != null || phone != null)
                {
                    command += " WHERE ";
                }
                Boolean needAnd = false;
                if (!String.IsNullOrWhiteSpace(namePattern))
                {
                    command += " ([PersonName] LIKE @Name) ";
                    cmd.Parameters.Add(new SqlParameter("@Name", namePattern));
                    needAnd = true;
                }
                if (!String.IsNullOrWhiteSpace(addressPattern))
                {
                    command += (needAnd ? " AND " : "") + " ([PersonAddress] LIKE @Address) ";
                    needAnd = true;
                    cmd.Parameters.Add(new SqlParameter("@Address", addressPattern));
                }
                if (birthDate != null)
                {
                    command += (needAnd ? " AND " : "") + " ([BirthDate] = @BD) ";
                    cmd.Parameters.Add(new SqlParameter("@BD", birthDate));
                }
                if (!String.IsNullOrWhiteSpace(phone))
                {
                    command += (needAnd ? " AND " : "") + " ([Phone] LIKE @Phone) ";
                    cmd.Parameters.Add(new SqlParameter("@Phone", phone));
                }
                cmd.CommandText = command;
                cmd.Connection = c;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(data);
                Collection<Contact> result = new Collection<Contact>();
                foreach (DataRow r in data.AsEnumerable())
                {
                    City city = null;
                    if (!(r.ItemArray[3] is DBNull))
                    {
                        city = new City(
                        (int)r.ItemArray[3],
                        (string)r.ItemArray[4],
                        (int)r.ItemArray[5],
                        (int)r.ItemArray[6]);
                    }
                    Person person = new Person(
                        (int)r.ItemArray[7],
                        (string)r.ItemArray[8],
                        (DateTime)r.ItemArray[9],
                        (string)r.ItemArray[10],
                        (string)r.ItemArray[11]);

                    ContactType type = ContactType.Mobile;
                    switch((string) r.ItemArray[2])
                    {
                        case "c":
                            type = ContactType.Corp; break;
                        case "h":
                            type = ContactType.Home; break;
                        case "m":
                        default:
                            break;
                    }
                    result.Add(new Contact(
                        (int)r.ItemArray[0],
                        (string)r.ItemArray[1],
                        type,
                        city,
                        person)
                    );
                }
                return result;
            }
        }

        public void Persist(Person person)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                c.Open();
                SqlCommand cmd;
                if (person.Id != -1)
                {
                    cmd = new SqlCommand(
                        "UPDATE " + PersonTable + " SET [PersonName] = @Name, [BirthDate] = @BD," +
                        " [PersonComment] = @Comment, [PersonAddress] = @Address WHERE [PersonId] = @ID", c);
                }
                else
                {
                    cmd = new SqlCommand(
                        "INSERT INTO " + PersonTable + " ([PersonName], [BirthDate], [PersonComment], [PersonAddress]) " +
                        " VALUES (@Name, @BD, @Comment, @Address)", c);
                }
                cmd.Parameters.Add(new SqlParameter("@ID", person.Id));
                cmd.Parameters.Add(new SqlParameter("@Name", person.Name));
                cmd.Parameters.Add(new SqlParameter("@Address", person.Address));
                cmd.Parameters.Add(new SqlParameter("@BD", person.BirthDate));
                cmd.Parameters.Add(new SqlParameter("@Comment", person.Comment));
                cmd.ExecuteNonQuery();
            }
        }

        public void Persist(City city)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                c.Open();
                SqlCommand cmd;
                if (city.Id != -1)
                {
                    cmd = new SqlCommand(
                        "UPDATE " + CityTable + " SET [CityName] = @Name, [CityCode] = @CityCode," +
                        " [StateCode] = @StateCode WHERE [CityId] = @ID", c);
                }
                else
                {
                    cmd = new SqlCommand(
                        "INSERT INTO " + CityTable + " ([CityName], [CityCode], [StateCode]) " +
                        " VALUES (@Name, @CityCode, @StateCode)", c);
                }
                cmd.Parameters.Add(new SqlParameter("@ID", city.Id));
                cmd.Parameters.Add(new SqlParameter("@Name", city.Name));
                cmd.Parameters.Add(new SqlParameter("@CityCode", city.CityCode));
                cmd.Parameters.Add(new SqlParameter("@StateCode", city.StateCode));
                cmd.ExecuteNonQuery();
            }
        }

        public void Persist(Contact contact)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                c.Open();
                SqlCommand cmd;
                if (contact.Id != -1)
                {
                    cmd = new SqlCommand(
                        "UPDATE " + ContactTable + " SET [Phone] = @Phone, [ContactType] = @Type, " +
                        " [Person] = @Person, [City] = @City WHERE [ContactId] = @ID", c);
                }
                else
                {
                    cmd = new SqlCommand(
                        "INSERT INTO " + ContactTable + " ([Phone], [ContactType], [Person], [City]) " +
                        " VALUES (@Phone, @Type, @Person, @City)", c);
                }
                cmd.Parameters.Add(new SqlParameter("@ID", contact.Id));
                cmd.Parameters.Add(new SqlParameter("@Phone", contact.Phone));
                string type = contact.Type == ContactType.Home ? "h" : contact.Type == ContactType.Corp ? "c" : "m";
                cmd.Parameters.Add(new SqlParameter("@Type", type));
                cmd.Parameters.Add(new SqlParameter("@Person", contact.Person != null && contact.Person.Id != -1 ? (object)contact.Person.Id : DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@City", contact.City != null && contact.City.Id != -1 ? (object)contact.City.Id : DBNull.Value));
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(City city)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                c.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM " + CityTable + " WHERE [CityID] = @ID", c);
                cmd.Parameters.Add(new SqlParameter("@ID", city.Id));
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(Person person)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                c.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM " + PersonTable + " WHERE [PersonID] = @ID", c);
                cmd.Parameters.Add(new SqlParameter("@ID", person.Id));
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(Contact contact)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                c.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM " + ContactTable + " WHERE [ContactID] = @ID", c);
                cmd.Parameters.Add(new SqlParameter("@ID", contact.Id));
                cmd.ExecuteNonQuery();
            }
        }

        public void RunSql(string sql)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                c.Open();
                String pattern = @"^GO";
                String[] elements = Regex.Split(sql, pattern, RegexOptions.Multiline);
                string message = "";
                foreach (var command in elements)
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(command, c);
                        cmd.ExecuteNonQuery();
                    } catch (Exception ex)
                    {
                        message += ex.Message + "\n";
                    }
                }
                if (!String.IsNullOrWhiteSpace(message))
                {
                    throw new Exception(message);
                }
            }
        }
    }

    public class Entity
    {
        public int Id { get; set; }
        public override bool Equals(object other)
        {
            return (other is Entity) && (other as Entity).Id == Id;
        }
        public override int GetHashCode()
        {
            return this.Id;
        }
    }

    public class City : Entity
    {
        public City(int id, string name, int cityCode, int stateCode)
        {
            Id = id;
            Name = name;
            CityCode = cityCode;
            StateCode = stateCode;
        }
        public City()
        {
            Id = -1;
        }
        public City(City p)
            : this(p.Id, p.Name, p.CityCode, p.StateCode)
        {
        }
        public string Name { get; set; }
        public int CityCode { get; set; }
        public int StateCode { get; set; }
    }

    public class Person : Entity
    {
        public Person(int id, string name, DateTime birthDate, string comment, string address)
        {
            Id = id;
            Name = name;
            BirthDate = birthDate;
            Comment = comment;
            Address = address;
        }
        public Person()
        {
            Id = -1;
            BirthDate = DateTime.Today;
        }
        public Person(Person p)
            : this(p.Id, p.Name, p.BirthDate, p.Comment, p.Address)
        {
        }
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public string Comment { get; set; }
        public string Address { get; set; }
    }

    public enum ContactType {
        Mobile, Corp, Home
    }

    public class Contact : Entity
    {
        public Contact()
        {
            Id = -1;
        }
        public Contact(int id, string phone, ContactType type, City city, Person person)
        {
            Id = id;
            Type = type;
            City = city;
            Person = person;
            Phone = phone;
        }
        public Contact(Contact p)
            : this(p.Id, p.Phone, p.Type, p.City, p.Person)
        {
        }
        public string Phone { get; set; }
        public ContactType Type { get; set; }
        public City City { get; set; }
        public Person Person { get; set; }
    }
}
