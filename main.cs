using System;
using System.Data.SqlClient;
//using sqlqb.SQLQueryBuilder;

namespace sqlqb {

   public class Program {
      public const string connectionString = "Server=localhost,1433;User Id=sa;Database=vault;Password=yourStrong(!)Password;TrustServerCertificate=yes;";

      public static void Main(/*string[] args*/) {
         SQLQueryBuilder.setConnectionString(connectionString);
         var reader = SQLQueryBuilder.select<Person>("SELECT * FROM person;");
         
         while(true) {
            var person = reader.read();
         }


      }


   }
}