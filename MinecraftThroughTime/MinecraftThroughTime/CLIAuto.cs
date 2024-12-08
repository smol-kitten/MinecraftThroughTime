/*

TODO

*/

namespace MinecraftThroughTime
{
    class CA
    {
        string[] commands = new string[] { "update", "make", "cache", "bake", "help"};
        string[] update = new string[] { "-f", "-j", "-i", "-v", "client", "server"};
        string[] make = new string[] { "-f", "-o", "-t", "-s", "-i", "-u" };
        string[] cache = new string[] { "clean", "open", "list" };
        string[] bake = new string[] { "full" };

        public void AutoComplete (string[] args){
            //match the first command to the commands array, if non match make console beep, if partial match BUT only one match, then auto complete, else list all possible matches
            
            if (args.Length == 1){
                //find partial matches
                List<string> matches = new List<string>();
                foreach (string command in commands){
                    if (command.StartsWith(args[0])){
                        matches.Add(command);
                    }                
                }
                if (matches.Count == 1){
                    Console.WriteLine(matches[0]);
                    return;
                }
                if (matches.Count > 1){
                    //save current line
                    //List all possible matches
                    foreach (string match in matches){
                        Console.WriteLine(match);
                    }
                    return;
                }
                Console.Beep();
                return;
            }


    }

}
