<%@ Page Language="C#" %>
<%@ Import Namespace="System.Xml.Linq" %>
<%@ Import Namespace="Silversite.Web.UI" %>

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
		<h3>A direct conversion of xaml &gt; png</h3> 
		<pre><code>&lt;img src="Homepage.xaml?png" runat="server" /&gt;</code></pre>
		<img src="Homepage.xaml?png" runat="server" />
		<hr />

		<h3>A conversion of multiple images with a batch script file, and of an animated gif.</h3>	
		<pre><code>&lt;img src="CreateImages.xic.xaml?image=Images/animated-loader.gif" runat="server" /&gt;</code></pre>
		<img src="CreateImages.xic.xaml?image=Images/animated-loader.gif" runat="server" />
		<hr />
		
		<h3>A html image map created from a svg source</h3>
		<p>This map was generated from <a href="http://en.wikipedia.org/wiki/Image:Map_of_USA_with_state_names.svg">"Map of USA with state names.svg"</a>.
		Note that in the current version the font is not resolved correctly.</p>
		<pre><code>&lt;asp:ImageMap ID="usamap" runat="server" ImageUrl="CreateImages.xic.xamll?image=Images/Usa.Map.png" CssClass="map" /&gt;</code></pre>
		<asp:ImageMap ID="usamap" runat="server" ImageUrl="CreateImages.xic.xaml?image=Images/Usa.Map.png" CssClass="map" />
		<hr />
		
		<h3>A PDF of the above homepage</h3>
		<pre><code>&lt;asp:HyperLink runat="server" NavigateUrl="Homepage.xaml?pdf"&gt;Homepage PDF&lt;/asp:HyperLink&gt;</code></pre>
		<asp:HyperLink runat="server" NavigateUrl="Homepage.xaml?pdf">Homepage PDF</asp:HyperLink>
		<hr />
		
		 <h3>Conversion of 3D content doesn't work under IIS (only on IIS6).</h3>
		<pre><code>&lt;img src="Kaxaml.xaml?gif&Storyboard=rotate&Frames=50" runat="server" /&gt;</code></pre>
		<img src="Kaxaml.xaml?gif&Storyboard=rotate&Frames=50" runat="server" />

		 <h3>Conversion of 3D content doesn't work under IIS, so here is a precompiled version.</h3>
		<pre><code>&lt;img src="Kaxaml.gif" runat="server" /&gt;</code></pre>
		<img src="Kaxaml.gif" runat="server" />

		<h3>Direct xaml in ASP.NET.</h3>
	
		<pre><code>
			&lt;asp:TextBox ID="buttontext" runat="server"&gt;I'm a button&lt;/asp:TextBox&gt;&lt;asp:Button ID="Button1" runat="server" OnClick="Update" Text="Update" /&gt;
			&lt;xic:XamlImage ID="dynamicbutton" runat="server" ImageUrl="~/XamlImageConverter.Demo/Images/XamlImage.png"&gt;
				&lt;Button&gt;I'm a Button&lt;/Button&gt;
			&lt;/xic:XamlImage&gt;
			&lt;script runat="server"&gt;
				protected void Update(object sender, EventArgs e) {
					var dynamicbutton = (XamlImage)Page.FindControl("dynamicbutton");
					dynamicbutton.Xaml = string.Format("&lt;Button&gt;{0}&lt;/Button&gt;", buttontext.Text);
				}
			&lt;/script&gt;
		</code></pre>
		
		 <asp:TextBox ID="TextBox1" runat="server">I'm a button</asp:TextBox><asp:Button ID="Button1" runat="server" OnClick="Update" Text="Update" />
		<xic:XamlImage ID="dynamicbutton" runat="server" ImageUrl="~/XamlImageConverter.Demo/Images/XamlImage.png" >
			<Button>I'm a Button</Button>
		</xic:XamlImage>
		<script runat="server">
				protected void Update(object sender, EventArgs e) {
					var dynamicbutton = (XamlImage)Page.FindControl("dynamicbutton"); 
					dynamicbutton.Xaml = string.Format("<Button>{0}</Button>", buttontext.Text);
				}
		 </script>
	</div>
	</form>
</body>
</html>
