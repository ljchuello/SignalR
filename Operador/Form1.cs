using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace Operador
{
    public partial class Form1 : Form
    {
        private HubConnection hubConnection;
        private HubConnectionState hubConnectionState = HubConnectionState.Disconnected;

        private Bitacora bitacora = new Bitacora();
        private List<Bitacora> bitacoras = new List<Bitacora>();

        private int i = 0;

        public Form1()
        {
            // Init
            InitializeComponent();

            // Deshabilitamos el error multiTask
            CheckForIllegalCrossThreadCalls = false;

            // Empezamos
            _ = Estado_Async();
            _ = Bitacora_Refrescar_Async();
            _ = Aniadir_Async();
        }

        async Task Aniadir_Async()
        {
            while (true)
            {
                // Esperamos
                await Task.Delay(2500);

                // Validamos la conexión
                if (hubConnectionState == HubConnectionState.Connected)
                {
                    // Mandamos una prueba
                    await hubConnection.SendAsync("SendMessage", "/test", $"{JsonConvert.SerializeObject(DateTime.UtcNow)}");
                    //await hubConnection.SendAsync("SendMessage", $"{Guid.NewGuid()}", $"{Guid.NewGuid()}");
                }
            }
        }

        async Task Bitacora_Refrescar_Async()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        // Título
                        Text = $"{hubConnectionState}";

                        // Seteamos a sólo 75
                        bitacoras = bitacoras.OrderByDescending(x => x.Exe).Take(75).ToList();

                        // Generamos
                        textBox1.Text = await bitacora.Gen_Async(bitacoras);

                        // Esperamos
                        Thread.Sleep(25);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            });
        }

        private async Task Estado_Async()
        {
            while (true)
            {
                try
                {
                    // Estado
                    hubConnectionState = hubConnection != null ? hubConnection!.State : HubConnectionState.Disconnected;

                    // Validamos
                    switch (hubConnectionState)
                    {
                        // Conectamos
                        case HubConnectionState.Disconnected:
                            hubConnection = new HubConnectionBuilder()
                                .WithUrl("https://ljchuello.xyz/chathub")
                                .Build();

                            // Escuchando
                            bitacora.Add(bitacoras, "Escuchando..");
                            hubConnection.On<string, string>("ReceiveMessage", (x, y) =>
                            {
                                i++;
                                bitacora.Add(bitacoras, $"{Diff(x, y)}");
                            });
                            await hubConnection.StartAsync();
                            break;

                        // Esperamos
                        case HubConnectionState.Reconnecting:
                        case HubConnectionState.Connecting:
                            await Task.Delay(5000);
                            break;

                        case HubConnectionState.Connected:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    bitacora.Add(bitacoras, $"Ah ocurrido un error; {ex.Message}");
                }
                await Task.Delay(25);
            }
        }

        string Diff(string x, string y)
        {
            try
            {
                DateTime enviado = JsonConvert.DeserializeObject<DateTime>(y);
                return $"Ping: {(DateTime.UtcNow - enviado).TotalMilliseconds:n0} ms";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return $"{x} - {y}";
            }
        }
    }
}