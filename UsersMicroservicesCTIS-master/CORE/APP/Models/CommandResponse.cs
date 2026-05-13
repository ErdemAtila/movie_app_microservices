namespace CORE.APP.Models
{
    // CommandResponse cr = new CommandResponse(); // false, null
    // CommandResponse cr1 = new CommandResponse()
    // {
    //      IsSuccessful = true,
    //      Message = "Success"
    // }; // 
    // CommandResponse cr2 = new CommandResponse(true);
    // CommandResponse cr3 = new CommandResponse(false , "Test");
    // CommandResponse cr3 = new CommandResponse(false , "Test", 17);
    // cr2.IsSuccessful = true; // this will not work because properties are readonly
    // Console.WriteLine(cr2.IsSuccessful);
    public class CommandResponse : Response // insert, update, delete
    {
        public bool IsSuccessful { get; } // readonly 

        public string Message { get; }

        public CommandResponse(bool isSuccessful, string message = "", int id = 0) : base(id)
        {
            IsSuccessful = isSuccessful;
            Message = message;
        }
    }
}
