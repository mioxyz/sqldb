using System;
using System.Data.SqlClient;
//using sqlqb.SQLQueryBuilder;
namespace sqlqb {
   public class Program {

      public static void waitForEnter() {
         Console.WriteLine("waiting for enter...");
         Console.ReadLine();
      }

      public const string connectionString = "Server=localhost,1433;User Id=sa;Database=vault;Password=yourStrong(!)Password;TrustServerCertificate=yes;";

      /*
         public int id = -1;
         public string first_name = null;
         public string last_name = null;
         public int age = -1;
      */

      public static void insertPersonsIntoDatabase() {
         const string query = "INSERT INTO person(first_name, last_name, age) VALUES ({0}, {1}, {2});";
      }

      public static void Main(/*string[] args*/) {
         SQLQueryBuilder.setConnectionString(connectionString);
         SQLQueryBuilder.register<Person>("person");
         var reader = SQLQueryBuilder.select<Person>("SELECT * FROM person;");
         while (true) {
            var person = reader.read();
            Console.WriteLine(person);
            if(person != null) SQLQueryBuilder.insert(person);
            if(null == person) break;
         }
         waitForEnter();
      }
   }
}
