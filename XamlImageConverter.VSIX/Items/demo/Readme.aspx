<%@ Page Language="C#" %>
<%@ Import Namespace="System.Xml.Linq" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>XamlImageConverter - Demo</title>
	<script type="text/javascript" src="jquery.js"></script>
	<script type="text/javascript" src="jquery.maphighlight.min.js"></script>
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
		<pre><code><img src="~/DemoXamlImageConverter/Homepage.xaml.png" runat="server" /></code></pre>
		<img src="~/DemoXamlImageConverter/Homepage.xaml.png" runat="server" />
		<hr />
			
		<h3>A conversion of multiple images with a batch script file, and of an animated gif.</h3>	
		<pre><code><img src="~/DemoXamlImageConverter/MakeImages.xic.xaml?image=~/DemoXamlImageConverter/Images/animated-loader.gif" runat="server" /></code></pre>
		<img src="~/DemoXamlImageConverter/MakeImages.xic.xaml?image=~/DemoXamlImageConverter/Images/animated-loader.gif" runat="server" />
		<hr />
		
		<h3>A html image map created from a svg source</h3>
		<p>This map was generated from <a href="http://en.wikipedia.org/wiki/Image:Map_of_USA_with_state_names.svg">"Map of USA with state names.svg"</a>.
		Note that in the current version the font is not resolved correctly.</p>
		<pre><code><asp:ImageMap ID="usamap" runat="server" ImageUrl="~/DemoXamlImageConverter/MakeImages.xic.xaml" CssClass="map" /></code></pre>
		<asp:ImageMap ID="usamap" runat="server" ImageUrl="~/DemoXamlImageConverter/MakeImages.xic.xaml" CssClass="map" />
		<hr />
		
		<h3>A PDF of the above homepage</h3>
		<pre><code><asp:HyperLink runat="server" NavigateUrl="~/DemoXamlImageConverter/Homepage.xaml.pdf">Homepage PDF</asp:HyperLink></code></pre>
		<asp:HyperLink runat="server" NavigateUrl="~/DemoXamlImageConverter/Homepage.xaml.pdf">Homepage PDF</asp:HyperLink>
		<hr />
		
		 <h3>Conversion of 3D content doesn't work under IIS (only on IIS6).</h3>
		<pre><code><img src="~/DemoXamlImageConverter/Kaxaml.xaml.gif?Storyboard=rotate&Frames=50" runat="server" /></code></pre>
		<img src="~/DemoXamlImageConverter/Kaxaml.xaml.gif?Storyboard=rotate&Frames=50" runat="server" />

		 <h3>Conversion of 3D content doesn't work under IIS, so here is a precompiled version.</h3>
		<pre><code><img src="~/DemoXamlImageConverter/Kaxaml.gif" runat="server" /></code></pre>
		<img src="~/DemoXamlImageConverter/Kaxaml.gif" runat="server" />

		<h3>Direct xaml in ASP.NET.</h3>
		 <asp:TextBox ID="buttontext" runat="server">I'm a button</asp:TextBox><asp:Button runat="server" OnClick="Update" Text="Update" />
	
		<pre><code>
			<asp:TextBox ID="buttontext" runat="!server ">I'm a button</asp:TextBox><asp:Button ID="Button1" runat="server" OnClick="Update" Text="Update" />
			<script runat="!server">
				protected void Update(object sender, EventArgs e) {
					xamlimage.Xaml = string.Format("<Button>{0}</Button>", new XText(buttontext.Text).ToString());
				}
			</script>
			<asp:XamlImage ID="xamlimage" runat="server" ImageUrl="~/DemoXamlImageConverter/Images/XamlImage.png">
				<Button>I'm a Button</Button>
			</asp:XamlImage>
		</code></pre>
		
		<asp:XamlImage ID="dymaicbutton" runat="server" ImageUrl="~/DemoXamlImageConverter/Images/XamlImage.png" >
			<Button>I'm a Button</Button>
		</asp:XamlImage>
		<script runat="server">
				protected void Update(object sender, EventArgs e) {
					XamlImageConverter.Xaml = string.Format("<Button>{0}</Button>", new XText(buttontext.Text).ToString());
				}
		 </script>
	</div>
	</form>
</body>
</html>
