using CORE.APP.Models;
using System.Globalization;

namespace CORE.APP.Services
{
    public abstract class ServiceBase
    {
        private CultureInfo _cultureInfo; // field

        protected CultureInfo CultureInfo // property
        { 
            get
            {
                return _cultureInfo;
            }
            set
            {
                _cultureInfo = value;
                Thread.CurrentThread.CurrentCulture = _cultureInfo;
                Thread.CurrentThread.CurrentUICulture = _cultureInfo;
            }
        }

        protected ServiceBase() // constructor
        {
            CultureInfo = new CultureInfo("en-US"); // tr-TR
        }

        // If needed, culture can be changed in a child class as below:
        // CultureInfo = new CultureInfo("tr-TR");

        protected CommandResponse Success(string message, int id) => new CommandResponse(true, message, id); // behavior

        protected CommandResponse Error(string message) => new CommandResponse(false, message); // behavior
    }
}
