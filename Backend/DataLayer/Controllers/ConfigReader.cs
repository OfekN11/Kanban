using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.DataLayer.Controllers
{
    class ConfigReader
    {
        private static ConfigReader reader = null;

        //fields
        private List<string> _forbiddenPasswords;

        public List<string> ForbiddenPasswords { get => _forbiddenPasswords; }

        private ConfigReader()
        {
            _forbiddenPasswords = File.ReadAllLines("ForbiddenPasswords.txt").ToList();
        }

        public static ConfigReader getInstance()
        {
            if (reader == null)
                reader = new ConfigReader();
            return reader;
        }
    }
}
