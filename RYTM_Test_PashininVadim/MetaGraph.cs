using System.Globalization;

namespace RYTM_Test_PashininVadim
{
    internal class Vertex
    {
        private int number;
        private double value;

        public int Number { get { return number; } }
        public double Value { get { return value; } set { this.value = value; } }

        public Vertex(int _number, int _value)
        {
            number = _number;
            value = _value;
        }
    }

    internal class Edge
    {
        private int number;
        private double value;
        private Vertex start;
        private Vertex end;

        public int Number { get { return number; } }
        public Vertex Start { get { return start; } }
        public Vertex End { get { return end; } }
        public double Value { get { return value; } set { this.value = value; } }


        public Edge(int _number, Vertex _start, Vertex _end, double _value)
        {
            number = _number;
            start = _start;
            end = _end;
            value = _value;
        }
    }
    public class MetaGraph
    {
        private List<Vertex> _verticies;
        private List<Edge> _edges;

        public MetaGraph()
        {
            _verticies = [];
            _edges = [];
        }

        /// <summary>
        /// Формирует мета-граф по заданному файлу
        /// </summary>
        /// <param name="fileInput"></param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void FormGraph(string fileInput)
        {
            if (!File.Exists(fileInput)) throw new FileNotFoundException(fileInput);

            // Считывание файла входных данных
            string[] lines = File.ReadAllLines(fileInput);
            
            int NV = 0; int NE = 0;
            var number = string.Empty;
            bool isNV = true;

            // Получение NV и NE
            foreach (var symbol in lines[0])
            {
                if (char.IsDigit(symbol))
                {
                    number += symbol;
                    continue;
                }
                if (isNV)
                {
                    isNV = false;
                    if (!int.TryParse(number, out NV)) throw new ArgumentException();
                    number = string.Empty;
                    continue;
                }
                if (!int.TryParse(number, out NE)) throw new ArgumentException();
            }

            _verticies = new List<Vertex>();

            for (int i = 1; i <= NV; i++)
                _verticies.Add(new Vertex(i, 0));
            _edges = new List<Edge> ();
            
            // Получение рёбер мета-графа
            for (int i = 1; i <= NE; i++)
            {
                (int start, int end, double value) = (0, 0, 0);
                var s1 = string.Empty;
                var s2 = string.Empty;
                bool isS1 = true;
                foreach (var symbol in lines[i + 1])
                {
                    if (char.IsDigit(symbol))
                    {
                        if (isS1) s1 += symbol;
                        else s2 += symbol;
                        continue;
                    }
                    isS1 = false;
                }
                if (!int.TryParse(s1, out start) || !int.TryParse(s2, out end)) return;
                if (!_verticies.Any(v => v.Number == start) || !_verticies.Any(v => v.Number == end)) return;
                _edges.Add(new Edge(i, _verticies.First(v => v.Number == start), _verticies.First(v => v.Number == end), value));
            }

            // Расчёт агент-функции
            var commands = new List<(string command, int number)>();
            for (int i = NE + 3; i < lines.Length; i++)
                commands.Add((lines[i], i - NE - 2));
            while (commands.Count > 0)
                DoCommand(commands, commands.First().number);
        }

        /// <summary>
        /// Записывает результат работы программы в файл
        /// </summary>
        /// <param name="fileOutput">Имя файла для записи результата</param>
        /// <exception cref="FileNotFoundException"></exception>
        public void WriteGraph(string fileOutput)
        {
            if (fileOutput == string.Empty) throw new FileNotFoundException(fileOutput);
            using (var output = new StreamWriter(fileOutput))
            {
                foreach (var v in _verticies)
                    output.WriteLine(v.Value.ToString(CultureInfo.InvariantCulture));
                foreach (var e in _edges)
                    output.WriteLine(e.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Метод выполнения правил в агент-функции
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="number"></param>
        private void DoCommand(IList<(string command, int number)> commands, int number)
        {
            if (!commands.Any(c => c.number == number)) return;
            var currentCommand = commands.First(c => c.number == number);
            commands.Remove(currentCommand);

            if (char.IsDigit(currentCommand.command.First()))
            {
                var strvalue = string.Empty;
                for (int i = 0; i < currentCommand.command.Length &&
                    (char.IsDigit(currentCommand.command[i]) || currentCommand.command[i] == '.'); i++)
                    strvalue += currentCommand.command[i];
                double.TryParse(strvalue, CultureInfo.InvariantCulture,out double value);
                if (number > _verticies.Count)
                {
                    _edges.First(e => e.Number == number - _verticies.Count).Value = value;
                }
                else
                {
                    _verticies.First(v => v.Number == number).Value = value;
                }
            }

            switch (currentCommand.command.First())
            {
                case 'v':
                {
                    var argstr = string.Empty;
                    int argnum = 0;
                    for (int i = 2; i < currentCommand.command.Length && char.IsDigit(currentCommand.command[i]); i++)
                        argstr += currentCommand.command[i];
                    if (!int.TryParse(argstr, out argnum) || !_verticies.Any(v => v.Number == argnum)) return;
                    if (commands.Any(c => c.number == argnum)) DoCommand(commands, argnum);
                    if (number > _verticies.Count)
                    {
                        _edges.First(e => e.Number == number - _verticies.Count).Value = _verticies.First(v => v.Number == argnum).Value;
                    }
                    else
                    {
                        _verticies.First(v => v.Number == number).Value = _verticies.First(v => v.Number == argnum).Value;
                    }
                    break;
                }
                case 'e':
                {
                    var argstr = string.Empty;
                    int argnum = 0;
                    for (int i = 2; i < currentCommand.command.Length && char.IsDigit(currentCommand.command[i]); i++)
                        argstr += currentCommand.command[i];
                    if (!int.TryParse(argstr, out argnum) || !_edges.Any(v => v.Number == argnum)) return;
                    if (commands.Any(c => c.number == argnum + _verticies.Count)) DoCommand(commands, argnum + _verticies.Count);
                    if (number > _verticies.Count)
                    {
                        _edges.First(e => e.Number == number - _verticies.Count).Value = _edges.First(v => v.Number == argnum).Value;
                    }   
                    else
                    {
                        _verticies.First(v => v.Number == number).Value = _edges.First(v => v.Number == argnum).Value;
                    }
                    break;
                    }
                case 'm':
                {
                    if (!currentCommand.command.StartsWith("min") || number > _verticies.Count) break;
                    var currVertice = _verticies.First(v => v.Number == number);
                    var incomingEdges = _edges.Where(e => e.End.Equals(currVertice)).ToList();
                    var min = double.MaxValue;
                    foreach ( var edge in incomingEdges )
                    {
                        if (commands.Any(c => c.number == edge.Number + _verticies.Count)) DoCommand(commands, edge.Number + _verticies.Count);
                        if (edge.Value < min) min = edge.Value;
                    }
                    _verticies.First(v => v.Number == number).Value = min;
                    break;
                }
                case '*':
                {
                    if (number <= _verticies.Count) break;
                    var currEdge = _edges.First(e => e.Number == number - _verticies.Count);
                    var currVertice = currEdge.Start;
                    if (commands.Any(c => c.number == currVertice.Number)) DoCommand(commands, currVertice.Number);
                    var incomingEdges = _edges.Where(e => e.End.Equals(currVertice)).ToList();
                    var product = currVertice.Value;
                    foreach ( var edge in incomingEdges )
                    {
                        if (commands.Any(c => c.number == edge.Number + _verticies.Count)) DoCommand(commands, edge.Number + _verticies.Count);
                        product *= edge.Value;
                    }
                    _edges.First(e => e.Number == number - _verticies.Count).Value = product;
                    break;
                }
                default: break;
            }
        }
    }
}
