using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Casino;
using Casino.TwentyOne;
using System.Data.SqlClient;
using System.Data;

namespace TwentyOne
{
    class Program
    {
        static void Main(string[] args)
        {
            const string casinoName = "Grand Hotel and Casino";

            Console.WriteLine("Welcome to the {0}. Let's start by telling me your name.", casinoName);
            string playerName = Console.ReadLine();
            if(playerName.ToLower() == "admin")
            {
                List<ExceptionEntity> ExceptionsLog = ReadExceptions();
                foreach(var exception in ExceptionsLog)
                {
                    Console.Write(exception.Id + " | ");
                    Console.Write(exception.ExceptionType + " | ");
                    Console.Write(exception.ExceptionMessage + " | ");
                    Console.Write(exception.TimeStamp + " | ");
                    Console.WriteLine();
                }
                Console.ReadLine();
                return;
            }
            bool validAnswer = false;
            int bank = 0;
            while (!validAnswer)
            {
                Console.WriteLine("And how much money did you bring today?");
                validAnswer = int.TryParse(Console.ReadLine(), out bank);
                if (!validAnswer)
                {
                    Console.WriteLine("Please enter digits only (No decimals).");
                }
            }
            
            Console.WriteLine("Hello, {0}. Would you like to join a game of 21 right now?", playerName);
            string answer = Console.ReadLine().ToLower();
            if (answer == "yes" || answer == "yeah" || answer == "y" || answer == "ya")
            {
                Player player = new Player(playerName, bank)
                {
                    Id = Guid.NewGuid()
                };
                using (StreamWriter file = new StreamWriter(@"C:\Users\ron_s\Documents\logs\log.txt", true))
                {

                    file.WriteLine(player.Id);

                }
                Game game = new TwentyOneGame();
                game += player;
                player.IsActivlyPlaying = true;
                while(player.IsActivlyPlaying && player.Balance > 0)
                {
                    try
                    {
                        game.Play();
                    }
                    catch (FraudException ex)
                    {
                        Console.WriteLine(ex.Message);
                        UpdateDbWithException(ex);
                        Console.ReadLine();
                        return;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("An error occurred. Please contact your System Administrator.");
                        UpdateDbWithException(ex);
                        Console.ReadLine();
                        return;
                    }
                }
                game -= player;
                Console.WriteLine("Thank you for playing");
            }
            Console.WriteLine("Feel free to look around the casino. Bye for now.");
            Console.Read();
        }

        private static void UpdateDbWithException(Exception ex)
        {
            string conectionString = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = TwentyOneGame; 
                                       Integrated Security = True; Connect Timeout = 30; Encrypt = False; 
                                       TrustServerCertificate = False; ApplicationIntent = ReadWrite; 
                                       MultiSubnetFailover = False";

            string queryString = @"INSERT INTO ExceptionsLog 
                                    (ExceptionType, ExceptionMessage, TimeStamp) 
                                    VALUES
                                    (@ExceptionType, @ExceptionMessage, @TimeStamp)";

            using (SqlConnection connection = new SqlConnection(conectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                command.Parameters.Add("@ExceptionType", SqlDbType.VarChar);
                command.Parameters.Add("@ExceptionMessage", SqlDbType.VarChar);
                command.Parameters.Add("@TimeStamp", SqlDbType.DateTime);

                command.Parameters["@ExceptionType"].Value = ex.GetType().ToString();
                command.Parameters["@ExceptionMessage"].Value = ex.Message;
                command.Parameters["@TimeStamp"].Value = DateTime.Now;

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }


        private static List<ExceptionEntity> ReadExceptions()
        {
            string conectionString = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = TwentyOneGame; 
                                       Integrated Security = True; Connect Timeout = 30; Encrypt = False; 
                                       TrustServerCertificate = False; ApplicationIntent = ReadWrite; 
                                       MultiSubnetFailover = False";
            
            string queryString = @"Select Id, ExceptionType, ExceptionMessage, TimeStamp From Exceptions";

            List<ExceptionEntity> ExceptionsLog = new List<ExceptionEntity>();

            using (SqlConnection connection = new SqlConnection(conectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ExceptionEntity exception = new ExceptionEntity()
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ExceptionType = Convert.ToString(reader["ExceptionType"]),
                        ExceptionMessage = Convert.ToString(reader["ExceptionMessage"]),
                        TimeStamp = Convert.ToDateTime(reader["TimeStamp"]),
                    };
                    ExceptionsLog.Add(exception);
                };
                connection.Close();
            }
            return ExceptionsLog;
        }
    }
}
