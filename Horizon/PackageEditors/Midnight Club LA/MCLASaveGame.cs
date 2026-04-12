using System;
using System.Collections.Generic;
using System.IO;

namespace Rockstar
{
    internal struct MCLASaveEntry
    {
        internal string EntryStringID;
        internal byte BuildVersion;
        internal uint DataSize;
        internal long Position;
    }

    internal class MCLACareer
    {
        private readonly EndianIO _io;
        internal int Money;
        internal int Reputation; // Adicionado: Reputation Points (RP)
        internal List<MCLAVehicle> Vehicles = new List<MCLAVehicle>();

        // Usaremos esta variável para guardar os dados da seção "Career" que não vamos mexer
        private byte[] _careerRawData;

        // Variável adicionada para guardar o offset real da Carreira sem causar o erro CS1061
        private long _careerStartPos;

        // ==========================================
        // TABELA DE RANKS (Baseado no IDA Dump)
        // ==========================================
        public static readonly Dictionary<string, int> ReputationRanks = new Dictionary<string, int>()
        {
            { "Rank 1: Backseat Driver", 0 },
            { "Rank 2: Navigator", 1000 },
            { "Rank 3: Student Driver", 2500 },
            { "Rank 4: Rookie", 4500 },
            { "Rank 5: Driver", 7000 },
            { "Rank 6: Racer", 10000 },
            { "Rank 7: Veteran", 14000 },
            { "Rank 8: Elite Racer", 19000 },
            { "Rank 8.5: Champion", 25000 },
            { "Rank 9: Legend", 32000 },
            { "Rank 10: Savant", 40000 },
            { "Rank 10.5: Hero", 50000 },
            { "Rank 11: Idol", 75000 } // Valor alto para garantir o Rank Máximo
        };

        internal MCLACareer(EndianIO io, long careerPosition, uint careerSize)
        {
            _io = io;

            // Salva a posição original para podermos escrever a Carreira no lugar certo depois
            _careerStartPos = io.Position;

            // Lemos a seção Career inteira apenas para manter os dados intactos ao salvar
            _careerRawData = io.In.ReadBytes((int)careerSize);

            // RP (Reputation Points) geralmente fica adjacente ao dinheiro (tentativa em 0x50FC0)
            io.Position = 0x50FC0;
            Reputation = io.In.ReadInt32();

            // O HTML nos disse que o dinheiro fica no offset absoluto 0x50FC4 do arquivo
            io.Position = 0x50FC4;
            Money = io.In.ReadInt32();

            // O HTML nos disse que os carros ficam a partir do offset 0x4785
            io.Position = 0x4785;

            // A garagem tem 30 slots no máximo
            for (int i = 0; i < 30; i++)
            {
                // Cada carro ocupa 0x1FF0 (8176 bytes)
                byte[] carData = io.In.ReadBytes(0x1FF0);

                MCLAVehicle vehicle = new MCLAVehicle(carData, i);

                // Só adiciona na lista se não for uma vaga vazia
                if (vehicle.ModelName != "Vaga Vazia")
                {
                    Vehicles.Add(vehicle);
                }
            }
        }

        internal void Write()
        {
            // Grava a seção Career original de volta (preservando os Unlocks se feitos)
            _io.Position = _careerStartPos;
            _io.Out.Write(_careerRawData);

            // Salva a Reputação
            _io.Position = 0x50FC0;
            _io.Out.Write(Reputation);

            // Salva o dinheiro no offset exato
            _io.Position = 0x50FC4;
            _io.Out.Write(Money);

            // Salva os carros de volta no offset da garagem
            _io.Position = 0x4785;
            foreach (var v in Vehicles)
            {
                // Calcula a posição do slot específico deste carro
                _io.Position = 0x4785 + (v.SlotIndex * 0x1FF0);
                _io.Out.Write(v.RawData);
            }
        }

        // ==========================================================
        // UNLOCK ALL (Baseado no Assembly sub_82388340 e .rdata)
        // ==========================================================
        public void UnlockAll()
        {
            // O intervalo 0x110 a 0x220 no bloco Career cobre as flags 
            // byte_8288E5B9 até byte_8288E5BE e as secundárias citadas no IDA.
            for (int i = 0; i < _careerRawData.Length; i++)
            {
                // Aplicamos 0x01 (True) na região de progressão e unlocks
                if (i >= 0x110 && i <= 0x220)
                {
                    _careerRawData[i] = 0x01;
                }
            }
        }
    }

    internal class MCLAVehicle
    {
        public int SlotIndex { get; set; }
        public string ModelName { get; set; }
        public string LicensePlate { get; set; }
        public byte[] RawData { get; set; }

        // ==========================================
        // DICIONÁRIOS DE HEX CODES DO FÓRUM
        // ==========================================
        public static readonly Dictionary<string, byte[]> SpeedMods = new Dictionary<string, byte[]>()
        {
            { "SPEED 0 (Stock)", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { "SPEED 1", new byte[] { 0x01, 0x00, 0x00, 0x01, 0x01, 0x01, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00 } },
            { "SPEED 2", new byte[] { 0x02, 0x00, 0x00, 0x00, 0x02, 0x02, 0x02, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02 } },
            { "SPEED 3", new byte[] { 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x01, 0x00 } },
            { "SPEED 4", new byte[] { 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00 } },
            { "SPEED 5", new byte[] { 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x01 } },
            { "SPEED 6", new byte[] { 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02 } },
            { "SPEED 7 (Instant 299)", new byte[] { 0x3A, 0x3A, 0x3A, 0x3A, 0x3A, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02 } },
            { "SPEED 8 (Max Glitch)", new byte[] { 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03 } },
            { "RS4 (Manual)", new byte[] { 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x02, 0x02, 0x02, 0x02 } },
            { "Suckin Handling", new byte[] { 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x00 } },
            { "Spinning Mod", new byte[] { 0x73, 0x66, 0x64, 0x6A, 0x77, 0x61, 0x73, 0x6C, 0x5A, 0x77, 0x4B, 0x65, 0x63 } },
            { "Can't Move Mod", new byte[] { 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0x05, 0x05, 0x05, 0x05 } },
            { "Fail", new byte[] { 0x0A, 0x0A, 0x0A, 0x0A, 0x0A, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F } }
        };

        public static readonly Dictionary<string, byte[]> NeonColors = new Dictionary<string, byte[]>()
        {
            { "BLACK (Off)", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { "PINK", new byte[] { 0x6E, 0x57, 0x48, 0x6D, 0xF1, 0x36, 0xB7, 0x13, 0x67, 0x67, 0xB1, 0x36 } },
            { "PINK (Rims Too)", new byte[] { 0x53, 0x64, 0xE4, 0xE6, 0x53, 0x6F, 0xEF, 0xF1, 0x3E, 0xC2, 0xC2, 0xC4 } },
            { "BLUE (Rims Too)", new byte[] { 0x9E, 0x40, 0xC0, 0xC2, 0x3D, 0x60, 0xE0, 0xE2, 0x53, 0x39, 0xB9, 0xBA } },
            { "YELLOW (Rims Too)", new byte[] { 0x53, 0x6A, 0xEA, 0xEC, 0x3D, 0x5D, 0x00, 0xD2, 0x53, 0x54, 0xD4, 0xD6 } },
            { "BLUE", new byte[] { 0x8D, 0x62, 0x8D, 0x6B, 0x87, 0xD2, 0x63, 0x70, 0x60, 0x64, 0x4D, 0xF5 } },
            { "BLUE (For Rim)", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x3C, 0x00, 0x80, 0x81, 0x3C, 0x00, 0x80, 0x81 } },
            { "RED", new byte[] { 0x60, 0xF1, 0x36, 0xB7, 0x13, 0x67, 0x67, 0xB1, 0x36, 0x6E, 0x57, 0x48 } },
            { "HELL BLUE", new byte[] { 0x3F, 0xB8, 0xB8, 0xB8, 0x78, 0x6D, 0xB8, 0xB8, 0x7F, 0xB0, 0xB8, 0xB8 } },
            { "WHITE", new byte[] { 0x53, 0x4E, 0xCE, 0xD0, 0x3E, 0x0C, 0x8C, 0x8D, 0x3D, 0xC0, 0xC0, 0xC2 } },
            { "GREEN", new byte[] { 0xF8, 0x67, 0x45, 0x35, 0x43, 0x16, 0x45, 0xEA, 0xEA, 0x53, 0xF2, 0xD1 } },
            { "CHANGEABLE", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x3B, 0xB6, 0xBD, 0xB1, 0x3E, 0xB4, 0xB4, 0xB5 } },
            { "KACK RED", new byte[] { 0x08, 0xB5, 0x44, 0xB3, 0xE5, 0x2B, 0xEF, 0xF3, 0xBF, 0x08, 0xA4, 0x3E } },
            { "YELLOW ORANGE", new byte[] { 0xA4, 0x98, 0x5E, 0xAF, 0xBE, 0xAF, 0xDE, 0xB4, 0xEF, 0xB8, 0x04, 0x98 } },
            { "DARK BLUE", new byte[] { 0xDA, 0xDD, 0xCA, 0xDA, 0xDE, 0xDA, 0xAC, 0xAA, 0xAC, 0xCA, 0xDA, 0xC0 } },
            { "ORANGE", new byte[] { 0x6F, 0xFA, 0x42, 0x89, 0x3F, 0xEA, 0xFA, 0x5C, 0xED, 0xAE, 0xAD, 0xAF } },
            { "DARK RED", new byte[] { 0x45, 0xA6, 0x70, 0xC9, 0xDA, 0x08, 0xC9, 0x78, 0xDA, 0x4C, 0x67, 0x90 } }
        };

        public MCLAVehicle(byte[] data, int slotIndex)
        {
            this.RawData = data;
            this.SlotIndex = slotIndex;

            // Offset 0x1FCF (Relativo): Nome do Carro
            this.ModelName = ExtractString(data, 0x1FCF, 33);

            // Offset 0x1FAF (Relativo): Placa
            this.LicensePlate = ExtractString(data, 0x1FAF, 32);
        }

        private string ExtractString(byte[] data, int offset, int maxLength)
        {
            List<char> chars = new List<char>();
            for (int i = 0; i < maxLength; i++)
            {
                if (offset + i >= data.Length) break;
                byte b = data[offset + i];

                if (b == 0x00 || b == 0xCD) break;

                if (b >= 32 && b <= 126)
                {
                    chars.Add((char)b);
                }
            }

            return chars.Count > 0 ? new string(chars.ToArray()) : "Vaga Vazia";
        }

        // ==========================================
        // MÉTODOS DE MODDING (Offsets Relativos Corretos)
        // ==========================================

        public void ApplySpeedMod(string modName)
        {
            if (SpeedMods.ContainsKey(modName))
            {
                byte[] speedMod = SpeedMods[modName];
                int speedOffset = 0x1B; // Offset 0x1B: Speed line

                if (RawData.Length > speedOffset + speedMod.Length)
                {
                    Array.Copy(speedMod, 0, RawData, speedOffset, speedMod.Length);
                }
            }
        }

        public void ApplyNeonMod(string colorName)
        {
            if (NeonColors.ContainsKey(colorName))
            {
                byte[] neonMod = NeonColors[colorName];
                int neonOffset = 0x1FBF; // Offset mapeado para Neon

                if (RawData.Length > neonOffset + neonMod.Length)
                {
                    Array.Copy(neonMod, 0, RawData, neonOffset, neonMod.Length);
                }
            }
        }

        public void SetRideHeight(byte heightValue)
        {
            // Offset Relativo 0x83C
            int heightOffset = 0x83C;

            if (RawData.Length > heightOffset + 1)
            {
                RawData[heightOffset] = heightValue;     // Frente
                RawData[heightOffset + 1] = heightValue; // Traseira
            }
        }

        public byte GetRideHeight()
        {
            int heightOffset = 0x83C;
            if (RawData != null && RawData.Length > heightOffset)
            {
                return RawData[heightOffset];
            }
            return 3;
        }
    }

    internal class MCLASaveGame
    {
        public readonly EndianIO _io;

        internal List<MCLASaveEntry> Entries;
        internal int GameVersion;
        internal int EntryCount;
        internal MCLACareer Career;

        internal MCLASaveGame(EndianIO io)
        {
            _io = io;
            Read();
        }

        internal void Save()
        {
            var careerEntry = Entries.Find(t => t.EntryStringID == "Career");
            _io.Position = careerEntry.Position;
            Career.Write();
        }

        internal void Read()
        {
            GameVersion = _io.In.ReadInt32();
            EntryCount = _io.In.ReadInt32();
            Entries = new List<MCLASaveEntry>();

            for (int i = 0; i < EntryCount; i++)
            {
                MCLASaveEntry saveEntry;
                saveEntry.EntryStringID = _io.In.ReadAsciiString(_io.In.ReadInt32());
                saveEntry.BuildVersion = _io.In.ReadByte();
                saveEntry.DataSize = _io.In.ReadUInt32();
                saveEntry.Position = _io.Position;
                _io.Position += saveEntry.DataSize;
                Entries.Add(saveEntry);
            }

            var careerEntry = Entries.Find(t => t.EntryStringID == "Career");
            _io.Position = careerEntry.Position;

            Career = new MCLACareer(_io, careerEntry.Position, careerEntry.DataSize);
        }
    }
}