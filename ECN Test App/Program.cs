using ECN;
using System;

namespace ECN_Test_App
{
    class Program
    {
        static void Main(string[] args)
        {
            /*ECNUser User = new ECNUser ();
            User.SetData ("Hello, World!", true, "G:");*/

            ECNListener Listener = new ECNListener ((user) => {
                Console.WriteLine ("Entered user '" + user.Name + "' with ID '" + user.ID + "' (" + (user.Verified ? "Verified" : "Not verified") + "). Data: " + user.Data);
            });

            Listener.SetUserLeaveHandler ((user) => {
                Console.WriteLine ("Leaved user '" + user.Name + "' with ID '" + user.ID + "' (" + (user.Verified ? "Verified" : "Not verified") + "). Data: " + user.Data);
            });

            Listener.ThreadListen (true);
        }
    }
}
