<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="XamlImageConverter.Web.Test._Default" AspCompat="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
		 
		<asp:HyperLink runat="server" Target="_blank" NavigateUrl="~/img/Kaxaml.xaml.png" Text="3D Image" />
		<hr />
	
		<asp:HyperLink runat="server" Target="_blank" NavigateUrl="~/img/XamlImageConverter.xaml.png" Text="2D Image" />
		<hr />
		
		<asp:HyperLink runat="server" Target="_blank" NavigateUrl="~/img/MakeImages.xic.xaml?image=~/img/animated-loader.gif" Text="Animated Gif" />
		
		<asp:HyperLink runat="server" Target="_blank" NavigateUrl="~/all.aspx" Text="All Images" />
	
    </div>
    </form>
</body>
</html>
