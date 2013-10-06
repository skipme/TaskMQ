
/*
*	Important ! 
*     this file preprocessed with t4 template - TextTemplatingFileGenerator
*
*   CodeGeneration parameters:
*       MessageType			'EMail'
*       RestApiBaseUri		'http://127.0.0.1:82/'
*
*/
namespace BulkMails
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

    public class EMail : TaskQueue.Providers.TaskMessage
    {
		const string MType = "EMail";
        public EMail()
            : base(MType)
        {

        }
        public EMail(TaskQueue.Providers.TaskMessage holder)
            : base(holder.MType)
        {
            this.SetHolder(holder.GetHolder());
        }
        public EMail(Dictionary<string, object> holder)
            : base(MType)
        {
            this.SetHolder(holder);
        }
		/// <summary>
		/// 	
		/// 
        /// </summary>	
		public string From { get; set; }
		/// <summary>
		/// 	
		/// 
        /// </summary>	
		public string FromAlias { get; set; }
		/// <summary>
		/// send to @	
		/// REQUIRED !
        /// </summary>	
		public string To { get; set; }
		/// <summary>
		/// some text to send	
		/// REQUIRED !
        /// </summary>	
		public string Body { get; set; }
		/// <summary>
		/// 	
		/// 
        /// </summary>	
		public string Subject { get; set; }
		/// <summary>
		/// 	
		/// 
        /// </summary>	
		public int SendErrors { get; set; }
		/// <summary>
		/// 	
		/// 
        /// </summary>	
		public string LastSError { get; set; }
	}
}