using System.Collections.Generic;
using System.Linq;

namespace telefonyDoAD
{
    /// <summary>
    /// store final changes. For rewrite data in AD.
    /// "attributes" store AD names of atributes to change. (in same order as attribData)
    /// "attribData" store data for these attributes.
    /// </summary>
    class telephoneUser
    {
        public string accountName { get; set; } = "";                       //user name
        public List<string> attributes { get; set; } = new List<string>();  //list of atributes
        public List<string> attribData { get; set; } = new List<string>();  //list of new values of that atributes

        public telephoneUser(string accountName)
        {
            this.accountName = accountName;
        }

        /// <summary>
        /// check if user have any stored changes
        /// </summary>
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
