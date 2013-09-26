using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkMails
{
    class Program
    {
        static void Main(string[] args)
        {
            // have some ldap catalog with 1000 persons and organisations
            // now we need to send them unique email
            
            // A:
            // 1. retrieve message model from platform by rest
            // 2. validate our email model with server email model

            // B:
            // 1. iterate items with email address
            // 2. send populated message(validated email model) by rest
        }
    }
}
