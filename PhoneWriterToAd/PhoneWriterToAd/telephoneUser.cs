using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace telefonyDoAD
{
    /// <summary>
    /// telephoneUser store final changes. For rewrite data in AD.
    /// "attributes" store AD names of atributes to change. (in same order as attribData)
    /// "attribData" store data for these attributes.
    /// </summary>
    class telephoneUser
    {
        public string accountName { get; set; } = "";
        public List<string> attributes { get; set; } = new List<string>();
        public List<string> attribData { get; set; } = new List<string>();

        public telephoneUser(string accountName)
        {
            this.accountName = accountName;
        }

        public bool haveChanges()
        {
            if (attributes.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
    }
}
