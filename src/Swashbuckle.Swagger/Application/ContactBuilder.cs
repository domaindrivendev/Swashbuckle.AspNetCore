using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class ContactBuilder
    {
        private string _name;
        private string _url;
        private string _email;

        public ContactBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public ContactBuilder Url(string url)
        {
            _url = url;
            return this;
        }

        public ContactBuilder Email(string email)
        {
            _email = email;
            return this;
        }

        internal Contact Build()
        {
            if ((_name ?? _url ?? _email) == null) return null;

            return new Contact
            {
                name = _name,
                url = _url,
                email = _email
            };
        }
    }
}