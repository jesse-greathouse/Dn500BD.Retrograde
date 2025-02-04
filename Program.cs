using Gdk;
using Gtk;
using System.IO;
using System.IO.Ports;

class MyWindow : Gtk.Window {
    public MyWindow() : base("DN 500BD Retrograde") {
    }

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}

class Hello {
    private static SerialPort mySerial;
    static Entry portBox;

    static string ReadData()
    {
        byte tmpByte;
        string rxString = "";
 
        tmpByte = (byte) mySerial.ReadByte();
 
        while (tmpByte != 255) {
            rxString += ((char) tmpByte);
            tmpByte = (byte) mySerial.ReadByte();
        }
 
        return rxString;
    }
 
    static bool SendData(string Data)
    {  
        mySerial.Write(Data);
        return true;
    }

     static void on_homeclick(object? sender, EventArgs args) {
        SendData("@0PCHM\r");
         //Console.WriteLine(ReadData());
        Console.WriteLine("up");
        return;
    }

    static void on_upclick(object? sender, EventArgs args) {
	SendData("@0PCCUSR3\r");
	//Console.WriteLine(ReadData());
        Console.WriteLine("up");
        return;
    }

        static void on_dnclick(object? sender, EventArgs args) {
		 SendData("@0PCCUSR4\r");        //Console.WriteLine(ReadData());

        Console.WriteLine("down");
        return;
    }
        static void on_lfclick(object? sender, EventArgs args) {
		 SendData("@0PCCUSR1\r");        //Console.WriteLine(ReadData());

        Console.WriteLine("left");
        return;
    }
        static void on_rtclick(object? sender, EventArgs args) {
		 SendData("@0PCCUSR2\r");        //Console.WriteLine(ReadData());

        Console.WriteLine("right");
        return;
    }

     static void on_connectclick(object? sender, EventArgs args) {
        if (mySerial != null)
            if (mySerial.IsOpen)
                mySerial.Close();
 
        mySerial = new SerialPort(portBox.Text, 115200);
        mySerial.Open();
        mySerial.ReadTimeout = 400;

        return;
    }

        static void on_entclick(object? sender, EventArgs args) {
		SendData("@0PCENTR\r");
        Console.WriteLine("enter");
        return;
    }

    static void Main() {


        Application.Init();

        // string[] ports = SerialPort.GetPortNames();
        portBox = new Entry();
        // ListStore store = new ListStore(typeof(string));

        //  store.AppendValues(ports);

        // portBox.Completion = new EntryCompletion ();
        // portBox.Completion.Model = store;
        // portBox.Completion.TextColumn = 0;

        MyWindow w = new MyWindow();
        VBox v = new VBox();
        HBox h = new HBox();

        h.Add(portBox);
        v.Add(h);

        h = new HBox();

        Button connect = new Button();
        connect.Label = "Connect";
        connect.Clicked += on_connectclick;

        h.Add(connect);
        v.Add(h);

        h = new HBox();

        h.Add(new Label());

        Button up = new Button();
        up.Label = "^";
        up.Clicked += on_upclick;

        h.Add(up);
        
        h.Add(new Label());
        v.Add(h);

        h = new HBox();

        Button lf = new Button();
        lf.Label = "<";
        lf.Clicked += on_lfclick;

        Button ent = new Button();
        ent.Label = "Enter";
        ent.Clicked += on_entclick;

        Button rt = new Button();
        rt.Label = ">";
        rt.Clicked += on_rtclick;

        h.Add(lf);
        h.Add(ent);
        h.Add(rt);
        v.Add(h);

        h = new HBox();
        h.Add(new Label());
        
        Button dn = new Button();
        dn.Label = "V";
        dn.Clicked += on_dnclick;

        h.Add(dn);
        h.Add(new Label());
       
        v.Add(h);

	h = new HBox();

        Button home = new Button();
        home.Label = "Home";
        home.Clicked += on_homeclick;

	h.Add(home);
	v.Add(h);

       
        w.Add(v);
        w.ShowAll();
        
        Application.Run();
    }
}
