<%@ Page Language="C#"  ValidateRequest="false" %>
<%@ Import Namespace="System.Xml.Linq" %>
<%@ Import Namespace="XamlImageConverter.Web.UI" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>XamlImageConverter - Demo</title>
	<script type="text/javascript" src="jquery.js"></script>
	<script type="text/javascript" src="jquery.maphilight.min.js"></script>
	<script type="text/javascript">
		$(function() {
			$('.map').maphilight();
		});
	</script>
</head>
<body>
	 <form id="form1" runat="server">
	 <div>
		
		<h3>A direct conversion of xaml =&gt; png</h3> 
		<pre><code>&lt;img src="Homepage.xaml?png" runat="server" /&gt;</code></pre>
		<img id="Img1" src="Homepage.xaml?png" runat="server" />
		<hr />

		 <h3>A PDF of the above xaml</h3>
		<pre><code>&lt;a href="Homepage.xaml?pdf"&gt;Homepage PDF&lt;/a&gt;</code></pre>
		<a href="Homepage.xaml?pdf">Homepage PDF</a>
		<hr />

		<h3>Rendering of multiple images with a batch script file, and of an animated gif.</h3>	
		<pre><code>&lt;img src="CreateImages.xic.xaml?Image=Images/Homepage/animated-loader.gif" runat="server" /&gt;</code></pre>
		<img id="Img2" src="CreateImages.xic.xaml?Image=Images/Homepage/animated-loader.gif" runat="server" />
		<hr />
		
		<h3>A html image map created from a svg source</h3>
		<p>This map was generated from <a href="http://en.wikipedia.org/wiki/Image:Map_of_USA_with_state_names.svg">"Map of USA with state names.svg"</a>.
		Note that in the current version the SVG font is not resolved correctly.</p>
		<pre><code>&lt;asp:ImageMap ID="usamap" runat="server" ImageUrl="CreateImages.xic.xaml?Image=Images/Usa.Map.png" CssClass="map" /&gt;</code></pre>
		 and in CreateImages.xic.xaml:
		<pre><code>&lt;xic:Snapshot Image="Usa.Map.png" &gt;
&lt;xic:ImageMap ID="usamap" Image="Usa.Map.png" File="Readme.aspx" &gt;
  &lt;HotSpots Elements="WA,OR,CA,AK,ID,NV,AZ,UT,MT,WY,CO,NM,TX,OK,KS,NE,SD,ND,MN,IA,MO,AR,LA,WI,IL,TN,MS,MI,IN,KY,AL,FL,GA,SC,NC,VA,WV,OH,PA,MD,NJ,NY,CT,MA,VT,NH,ME,RI,DE,HI" HotSpotMode="PostBack" PostBackValue="%ID%"/&gt;
&lt;/xic:ImageMap&gt
			  </code></pre>

		<asp:ImageMap ID="usamap" runat="server" ImageUrl="CreateImages.xic.xaml?Image=Images/Usa.Map.png" CssClass="map" />
		<hr />
		
		 <h3>Rendering of 3D content doesn't work under IIS (only on IIS6 & IISExpress, because IIS does not run in a user session and can't access video drivers).</h3>
		<pre><code>&lt;img src="Kaxaml.xaml?gif&Storyboard=rotate&Frames=50&Loop=0" runat="server" /&gt;</code></pre>
		<img id="Img3" src="Kaxaml.xaml?gif&Storyboard=rotate&Frames=50&Loop=0" runat="server" />

		 <h3>Rendering of 3D content doesn't work under IIS, so here is a precompiled version.</h3>
		<pre><code>&lt;img src="Kaxaml.gif" runat="server" /&gt;</code></pre>
		<img src="Kaxaml.gif" runat="server" />

		<h3>Direct xaml in ASP.NET.</h3>
	
		<pre><code>&lt;asp:TextBox ID="buttontext" runat="server"Width="700" Height="100" TextMode="MultiLine"&gt;&lt;Button&gt;I'm a button&lt;/Button&gt;&lt;/asp:TextBox&gt;
&lt;asp:Button runat="server" OnClick="Update" Text="Update" /&gt;
&lt;xic:XamlImage ID="dynamicbutton" runat="server" Image="Images/XamlImage.png" &gt;
  &lt;Button&gt;I'm a Button&lt;/Button&gt;
&lt;/xic:XamlImage&gt;
&lt;script runat="server"&gt;
  protected void Update(object sender, EventArgs e) {
    var dynamicbutton = (XamlImage)Page.FindControl("dynamicbutton");
    dynamicbutton.Content = buttontext.Text;
  }
&lt;/script&gt;
		</code></pre>
		 		
		 <asp:TextBox ID="buttontext" runat="server" Width="700" Height="100" TextMode="MultiLine">&lt;Button&gt;I'm a button&lt;/Button&gt;</asp:TextBox><br />
		 You can also insert xaml tags here.<br />
		 <asp:Button runat="server" OnClick="Update" Text="Update" /><br />
		<xic:XamlImage ID="dynamicbutton" runat="server" Image="~/XamlImageConverter.Demo/Images/XamlImage.png" >
			<Button>I'm a Button</Button>
		</xic:XamlImage>
		<script runat="server">
				protected void Update(object sender, EventArgs e) {
					var dynamicbutton = (XamlImage)Page.FindControl("dynamicbutton"); 
					dynamicbutton.Content = buttontext.Text;
				}
		 </script>
	</div>
	</form>
</body>
</html>
