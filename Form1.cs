using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace bomberwii
{
    public partial class Form1 : Form
    {
        #region Conversão de Little pra Big Endian

        //Coloque em todos os seus projetos dessa forma

        uint bigendian16(uint le)
        {
            //Converte um valor de 16Bits de Big endian para Little endian

            uint be = (uint)((byte)(le >> 8) | ((byte)le << 8));
            return be;
        }

        uint bigendian32(uint le)
        {
            //Converte um valor de 32Bits de Big endian para Little endian

            uint pr = le >> 24;
            uint se = le >> 8 & 0x00FF00;
            uint te = le << 24;
            uint qu = le << 8 & 0x00FF0000;
            return pr | se | te | qu;
        }

        ulong bigendian64(ulong le)
        {
            //Converte um valor de 64Bits de Big endian para Little endian

            ulong primeiroByte = (le >> 0) & 0xFF;
            ulong segundoByte = (le >> 8) & 0xFF;
            ulong terceiroByte = (le >> 16) & 0xFF;
            ulong quartoByte = (le >> 24) & 0xFF;
            ulong quintoByte = (le >> 32) & 0xFF;
            ulong sextoByte = (le >> 40) & 0xFF;
            ulong setimoByte = (le >> 48) & 0xFF;
            ulong oitavoByte = (le >> 56) & 0xFF;
            return oitavoByte | setimoByte | sextoByte | quintoByte | quartoByte | terceiroByte | segundoByte | primeiroByte;
        }

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
//___________________________________________________________________________________________________________________________________

            //Essa parte que começa aqui você pode sempre copiar e colar, modificando só a parte do arquivo
            //Você só precisa modificar o tipo do arquivo na linha openFileDialog1.Filter
            //E modificar na linha openFileDialog1.Title

            OpenFileDialog openFileDialog1 = new OpenFileDialog(); // Cria uma variavel para abrir a caixa de seleção de arquivo 
            openFileDialog1.Filter = "Arquivo Bomberman Wii|*.bin|Todos os arquivos (*.*)|*.*"; //Expecifica um tipo de arquivo a ser aberto
            openFileDialog1.Title = "Escolha um arquivo do jogo Bomberman..."; //Uma mensagem que ficará na caixa
            openFileDialog1.Multiselect = true; //Aqui se for true, será possivel selecionar mais de um arquivo. Se for false, só será possivel selecionar um unico arquivo.

            if (openFileDialog1.ShowDialog() == DialogResult.OK) //Caso o usuário escolha um arquivo.
            {
                int arquivosabertos = openFileDialog1.FileNames.Count(); //Aqui o programa guarda todos os nomes dos arquivos selecionados pelo usuário

                foreach (String file in openFileDialog1.FileNames) //Aqui é o comando que vai executar o programa pra cada arquivo.
                {
                    using (FileStream stream = File.Open(file, FileMode.Open)) //Aqui to avisando ao sistema que é pra ele abrir o arquivo
                    {
                        BinaryReader br = new BinaryReader(stream); //Ponteiro que faz a leitura do arquivo
                        BinaryWriter bw = new BinaryWriter(stream); //Ponteiro que faz escrita no arquivo
//___________________________________________________________________________________________________________________________________

                        uint quantidadetextos = bigendian32(br.ReadUInt32()); //Lê e guarda a quantidade de textos

                        uint[] ponteiros = new uint[quantidadetextos]; //Cria um array pra salvar cada um dos ponteiros

                        uint[] tamanho_da_frase = new uint[quantidadetextos]; //Cria um array pra salvar o tamanho de cada uma das frases

                        FileInfo fi = new FileInfo(file);  //Falo ao sistema que quero informações sobre o arquivo
                        long tamanho_do_arquivo = fi.Length; //cria a variavel e guarda o tamanho do arquivo

                        //Aqui o programa vai fazer a mesma coisa pra cada um dos ponteiros
                        for (int i = 0; i < quantidadetextos; i++) //Vai fazer a mesma coisa enquanto i for menor que a quantidade de textos
                        {
                            ponteiros[i] = bigendian32(br.ReadUInt32()); //Lê o ponteiro e guarda na posição i
                        }

                        //Aqui o programa vai fazer o calculo do tamanho de cada texto
                        //Para fazer o calculo bata pegar o ponteiro posterior e subtrair o atual
                        //Ou seja, o segundo ponteiro menos o primeiro por exemplo
                        //o primeiro ponteiro está na posição 0 e o segundo está na posição 1; i começa como zero...
                        //por isso [i+1] - i
                        //Precisamos saber a quantidade de bytes na frase para criar o array q vai guardar os bytes
                        for (int i = 0; i < quantidadetextos; i++) //Vai fazer a mesma coisa enquanto i for menor que a quantidade de textos
                        {
                            if (i < quantidadetextos - 1) //enquanto i for menor que a quantidade de textos, ele repete esse calculo
                            {
                                tamanho_da_frase[i] = ponteiros[i + 1] - ponteiros[i];
                            }
                            else //quando a quantidade de textos for alcançada, ele faz a conta do ultimo ponteiro
                            {
                                tamanho_da_frase[i] = (uint)tamanho_do_arquivo - ponteiros[i];
                            }
                        }

                        string todosOsTextos = "";//Cria a variavel para salvar todos os textos antes de colocar no arquivo

                        for (int i = 0; i < quantidadetextos; i++)
                        {
                            br.BaseStream.Seek(ponteiros[i], SeekOrigin.Begin); //Vai pro endereço do texto

                            byte[] texto = new byte[tamanho_da_frase[i]];//Cria o array que vai guardar cada byte do texto - por isso temos que saber a quantidade de bytes na frase

                            for (int j = 0; j < tamanho_da_frase[i]; j++)//Aqui o programa vai ler byte por byte e guardar no array
                            {
                                texto[j] = br.ReadByte();
                            }

                            string texto_convertido = Encoding.GetEncoding("shift_jis").GetString(texto);//A variavel texto_convertido recebe o texto convertido

                            texto_convertido = texto_convertido.Replace("\0", "[00]").Replace("\x0A", "[ql]").Replace("\x01", "[01]").Replace("\x03", "[03]").Replace("\x06", "[06]").Replace("\x08", "[08]").Replace("\x0B", "[0B]");//Apaga os 00 do texto

                            todosOsTextos += texto_convertido + "\r\n";//Acrescenta no texto <END> e quebra de linha
                        }
                        //Escreve no arquivo tudo que está salvo na variavel todosOsTextos
                        File.WriteAllText(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".txt", todosOsTextos);
                    }
                }
                MessageBox.Show("Terminado!", "AVISO!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
//___________________________________________________________________________________________________________________________________

            //Essa parte que começa aqui você pode sempre copiar e colar, modificando só a parte do arquivo
            //Você só precisa modificar o tipo do arquivo na linha openFileDialog1.Filter
            //E modificar na linha openFileDialog1.Title

            OpenFileDialog openFileDialog1 = new OpenFileDialog(); // Cria uma variavel para abrir a caixa de seleção de arquivo 
            openFileDialog1.Filter = "Arquivo Bomberman Wii|*.bin|Todos os arquivos (*.*)|*.*"; //Expecifica um tipo de arquivo a ser aberto
            openFileDialog1.Title = "Escolha um arquivo do jogo Bomberman..."; //Uma mensagem que ficará na caixa
            openFileDialog1.Multiselect = true; //Aqui se for true, será possivel selecionar mais de um arquivo. Se for false, só será possivel selecionar um unico arquivo.

            if (openFileDialog1.ShowDialog() == DialogResult.OK) //Caso o usuário escolha um arquivo.
            {
                int arquivosabertos = openFileDialog1.FileNames.Count(); //Aqui o programa guarda todos os nomes dos arquivos selecionados pelo usuário

                foreach (String file in openFileDialog1.FileNames) //Aqui é o comando que vai executar o programa pra cada arquivo.
                {
                    using (FileStream stream = File.Open(file, FileMode.Open)) //Aqui to avisando ao sistema que é pra ele abrir o arquivo
                    {
                        BinaryReader br = new BinaryReader(stream); //Ponteiro que faz a leitura do arquivo
                        BinaryWriter bw = new BinaryWriter(stream); //Ponteiro que faz escrita no arquivo
//___________________________________________________________________________________________________________________________________

                        //Verifica se o arquivo TXT existe
                        FileInfo dump = new FileInfo(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".txt");

                        string filename = Path.GetFileNameWithoutExtension(file);

                        if (dump.Exists) //Se o arquivo TXT existe
                        {
                            //Lê e guarda na variavel TXT
                            var txt = File.ReadLines(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".txt");

                            uint quantidadetextos = bigendian32(br.ReadUInt32()); //Lê e guarda a quantidade de textos

                            uint ponteiro = bigendian32(br.ReadUInt32()); //Lê o primeiro ponteiro

                            stream.SetLength(ponteiro); //Apaga tudo que estiver depois do bloco de ponteiros

                            int numerolinha = 0; //Inicia a variável que vai contar as linhas

                            uint novoponteiro = ponteiro;

                            foreach (var line in txt) //Para cada linha no TXT
                            {
                                bw.BaseStream.Seek(4 + 4 * numerolinha, SeekOrigin.Begin); //Vai para o local escrever o ponteiro

                                bw.Write(bigendian32(novoponteiro)); //Escreve o novo ponteiro

                                //Substitui os caracteres
                                string texto = line.Replace("[00]", "\0").Replace("[ql]", "\x0A").Replace("[01]", "\x01").Replace("[03]", "\x03").Replace("[06]", "\x06").Replace("[08]", "\x08").Replace("[0B]", "\x0B");

                                byte[] bytes = Encoding.GetEncoding("shift_jis").GetBytes(texto); //Faz a transformação de texto em bytes

                                bw.BaseStream.Seek(novoponteiro, SeekOrigin.Begin); //Vai pra onde será escrito os bytes do texto

                                bw.Write(bytes); //Escreve os bytes do texto

                                novoponteiro += (uint)bytes.Length; //Pega o ponteiro e acrescenta o tamanho do texto

                                numerolinha++; //Conta uma linha
                            }

                        }
                        else //Se o arquivo TXT não existir
                        {
                            //Avisa que não encontrou o arquivo e encerra o programa
                            MessageBox.Show("O arquivo " + filename + ".txt não foi encontrado!", "AVISO!");

                            return; //Volta pro programa
                        }
                    }
                }
                MessageBox.Show("Terminado!", "AVISO!");
            }
        }
    }
}