using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GopherSharp {
	// Due to the simple nature of gopher, (no keepalive and other adv.
	// stuff) instead of a Client, we have a static request library.
	// This makes it easier to quickly fetch, as there is no client
	// object needed - Gopher wouldn't need a complex object.
	public static class GopherRequester {
		// nicked this off SO, terribly ugly but it should work
		private static byte[] ReadAllBytes(this Stream s)
		{
			const int bufferSize = 4096;
			using (var ms = new MemoryStream())
			{
		        byte[] buffer = new byte[bufferSize];
		        int count;
		        while ((count = s.Read(buffer, 0, buffer.Length)) != 0){
		            ms.Write(buffer, 0, count);
				}
		        return ms.ToArray();
		    }
		}
		
		public static byte[] RequestRaw(string server, string resource, int port = 70) {
			TcpClient tc = new TcpClient(server, port);
			using (Stream s = tc.GetStream()) {
				StreamWriter sw = new StreamWriter(s);
				sw.AutoFlush = true;
				
				sw.WriteLine(resource);
				
				return s.ReadAllBytes();
				
				//return new byte[0];
			}
		}
		
		public static string RequestString(string server, string resource, int port = 70) {
			TcpClient tc = new TcpClient(server, port);
			using (Stream s = tc.GetStream()) {
				StreamReader sr = new StreamReader(s);
				StreamWriter sw = new StreamWriter(s);
				sw.AutoFlush = true;
				
				sw.WriteLine(resource);
				
				return sr.ReadToEnd();
			}
		}
		
		public static List<GopherItem> RequestMenu(string server, string resource, int port = 70) {
			TcpClient tc = new TcpClient(server, port);
			using (Stream s = tc.GetStream()) {
				StreamReader sr = new StreamReader(s);
				StreamWriter sw = new StreamWriter(s);
				sw.AutoFlush = true;
				
				sw.WriteLine(resource);
				
				return GopherItem.MakeFromMenu(sr.ReadToEnd());
			}
		}
	}
	
	public class GopherItem {
		public char ItemType {
			get; set;
		}
		
		public string DisplayString {
			get; set;
		}
		
		public string Selector {
			get; set;
		}
		
		public string Hostname {
			get; set;
		}
		
		public int Port {
			get; set;
		}
		
		public GopherItem() {
			// create an empty information item
			ItemType = 'i';
			DisplayString = String.Empty;
			Selector = String.Empty;
			Hostname = "error.host"; // floodgap does this, correct?
			Port = 1;
		}
		
		public GopherItem(string raw) {
			string parsing = raw;
			// Get the item type
			ItemType = parsing.ToCharArray()[0];
			parsing = parsing.Remove(0, 1);
			// Split the strings
			string[] items = parsing.Split('\t');
			DisplayString = items[0];
			Selector = items[1];
			Hostname = items[2];
			Port = Convert.ToInt32(items[3]);
		}
		
		public override string ToString() {
			return ItemType.ToString() + DisplayString + "\t" + Selector
				+ "\t" + Hostname + "\t" + Port.ToString();
		}
		
		// Menu creation functions
		public static List<GopherItem> MakeFromMenu(string menu) {
			string[] bufferItems = Regex.Split(menu, "\r?\n");
			return MakeFromMenu(bufferItems);
		}
		
		public static List<GopherItem> MakeFromMenu(string[] menu) {
			List<GopherItem> items = new List<GopherItem>();
			
			foreach (string s in menu) {
				if (String.IsNullOrWhiteSpace(s))
					continue; //handle slightly broken sites
				if (s == ".")
					break;
				items.Add(new GopherItem(s));
			}
			return items;
		}
	}
}
