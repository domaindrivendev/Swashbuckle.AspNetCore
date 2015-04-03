using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class LicenseBuilder
    {
        private string _name;
        private string _url;

        public LicenseBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public LicenseBuilder Url(string url)
        {
            _url = url;
            return this;
        }

        internal License Build()
        {
            if ((_name ?? _url) == null) return null;

            return new License
            {
                name = _name,
                url = _url
            };
        }
    }
}