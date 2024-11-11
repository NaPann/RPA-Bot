namespace SNJ.RPA.App.Models
{
	public class ErrorViewModel
	{
		public string? RequestId { get; set; }

		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}

	public class Data_Request
	{
		public string Data { get; set; }
	}
}
