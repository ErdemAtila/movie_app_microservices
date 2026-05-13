namespace CORE.APP.Models
{
    // Response r = new Response(25);
    // Response 1 = new Response();
    public abstract class Response
    {
        public virtual int Id { get; set; }

        protected Response()
        {
        }

        protected Response(int id) // Response response = new Response(7);
        {
            Id = id;
        }
    }
}
