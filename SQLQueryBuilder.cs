using System;
using System.Data.SqlClient;

namespace sqlqb {
   public class SQLQueryBuilder {

      public class Reader<T> {
         public SqlConnection connection;
         public SqlDataReader reader;
         private static readonly Type typeString = typeof(String);
         private static readonly Type typeBool = typeof(bool);
         private static readonly Type typeInt32 = typeof(Int32);
         private static readonly Type typeInt64 = typeof(Int64);

         private Type type_;
         private readonly Type[] parameters;
         private readonly SqlCommand command;

         public Reader(SqlConnection connection, string query) {
            this.connection = connection;
            connection.Open();

            type_ = typeof(T);

            command = new SqlCommand(query, connection);

            reader = command.ExecuteReader();
            var fields = type_.GetFields().ToList();
            var parameters = new List<Type>();
            Console.WriteLine("before field loop");
            foreach(var field in fields) {
               Console.WriteLine(field.Name);
               parameters.Add(field.FieldType);
            }
            this.parameters = parameters.ToArray();
         }

         public T? read() {
            if(!reader.Read()) return default(T);

            object[] realizedParameters = new object[parameters.Length];
            var k = 0;
            Console.WriteLine("before parameter loop");
            foreach(var parameter in parameters) {
               Console.WriteLine(parameter.Name);
               if(parameter == typeBool) {
                  realizedParameters[k] = reader.GetBoolean(k);
               }else if(parameter == typeString) {
                  realizedParameters[k] = reader.GetString(k);
               }else if(parameter == typeInt32) {
                  realizedParameters[k] = reader.GetInt32(k);                  
               }else{
                  throw new NotImplementedException(parameter.ToString());
               }
               ++k;
            }
            var constructor = type_.GetConstructor(parameters);
            var something = (T)constructor.Invoke(realizedParameters);
            if(null == something) return default(T);
            return (T)something;
         }
      }

      private bool defaultInitialized = false;
      public static Dictionary<string, string> connectionStrings = new Dictionary<string, string>();
      public const string DEFAULT_CONNECTION_STRING_NAME = "__DEFAULT__";

      public static void setConnectionString(string connectionString, string? name = null) {
         connectionStrings[(null == name) ? DEFAULT_CONNECTION_STRING_NAME : name] = connectionString;
      }

      public static Reader<T> select<T>(string query, string connectionStringName = DEFAULT_CONNECTION_STRING_NAME) {
         var connection = new SqlConnection(connectionStrings[connectionStringName]);
         return new Reader<T>(connection, query);
      }

   }
}