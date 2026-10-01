using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Final281_Project_Prg
{
    
    // 1. INTERFACES
    

    internal interface ICommunityManager
    {
        void AddCommunity();
        void ViewCommunities();
        void EditCommunity();
        void DeleteCommunity();
        void ViewCommunityDetails();
    }

    internal interface IInputValidator
    {
        int GetValidInteger(string message);
        int GetValidRating(string message);
        string GetValidText(string message);
    }

    
    // 2. CUSTOM EXCEPTIONS
  

    internal class CommunityNotFoundException : Exception
    {
        public CommunityNotFoundException() : base("Community could not be found.") { }
        public CommunityNotFoundException(string message) : base(message) { }
        public CommunityNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    internal class InvalidPulseScoreException : Exception
    {
        public InvalidPulseScoreException() : base("Invalid pulse score.") { }
        public InvalidPulseScoreException(string message) : base(message) { }
        public InvalidPulseScoreException(string message, Exception inner) : base(message, inner) { }
    }

    
    // 3. EVENT ARGS
    

    internal class AnomalyDetectedEventArgs : EventArgs
    {
        public string CommunityName { get; }
        public string IndicatorName { get; }
        public int PreviousValue { get; }
        public int CurrentValue { get; }
        public double PercentageChange { get; }

        public AnomalyDetectedEventArgs(string communityName, string indicatorName, int previousValue, int currentValue)
        {
            CommunityName = communityName;
            IndicatorName = indicatorName;
            PreviousValue = previousValue;
            CurrentValue = currentValue;

            if (previousValue == 0)
            {
                PercentageChange = currentValue > 0 ? 100.0 : 0.0;
            }
            else
            {
                PercentageChange = ((double)(currentValue - previousValue) / previousValue) * 100.0;
            }
        }
    }

    internal class PulseLevelChangedEventArgs : EventArgs
    {
        public string CommunityName { get; }
        public string OldLevel { get; }
        public string NewLevel { get; }
        public int PulseScore { get; }

        public PulseLevelChangedEventArgs(string communityName, string oldLevel, string newLevel, int pulseScore)
        {
            CommunityName = communityName;
            OldLevel = oldLevel;
            NewLevel = newLevel;
            PulseScore = pulseScore;
        }
    }

    
    // 4. CORE DATA MODELS
    

    internal class Community
    {
        private string communityName;
        private int communityID;
        private int population;
        private string populationPressure;
        private string riskLevel;
        private int pulseScore;

        public string CommunityName
        {
            get { return communityName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Community name cannot be empty.");
                }
                communityName = value;
            }
        }

        public int CommunityID
        {
            get { return communityID; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Community ID cannot be negative.");
                }
                communityID = value;
            }
        }

        public int Population
        {
            get { return population; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Population cannot be negative.");
                }
                population = value;
            }
        }

        public string PopulationPressure
        {
            get { return populationPressure; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Population pressure cannot be empty.");
                }
                populationPressure = value;
            }
        }

        public string RiskLevel
        {
            get { return riskLevel; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Risk level cannot be empty.");
                }
                riskLevel = value;
            }
        }

        public int PulseScore
        {
            get { return pulseScore; }
            set
            {
                if (value < 0 || value > 100)
                {
                    throw new InvalidPulseScoreException("Pulse score must be between 0 and 100.");
                }
                pulseScore = value;
            }
        }

        public string PulseStatus
        {
            get
            {
                if (PulseScore <= 30) return "Normal";
                if (PulseScore <= 50) return "Watch";
                if (PulseScore <= 70) return "Elevated";
                if (PulseScore <= 85) return "High";
                return "Critical";
            }
        }

        public Community(string communityName, int communityID, int population, string populationPressure, string riskLevel, int pulseScore)
        {
            CommunityName = communityName;
            CommunityID = communityID;
            Population = population;
            PopulationPressure = populationPressure;
            RiskLevel = riskLevel;
            PulseScore = pulseScore;
        }
    }

    internal class Indicator
    {
        private string indicatorName;
        private int currentNumberOfIncidents;
        private int previousNumberOfIncidents;

        public string IndicatorName
        {
            get { return indicatorName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Indicator name cannot be empty.");
                }
                indicatorName = value;
            }
        }

        public int CurrentNumberOfIncidents
        {
            get { return currentNumberOfIncidents; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Current number of incidents cannot be negative.");
                }
                currentNumberOfIncidents = value;
            }
        }

        public int PreviousNumberOfIncidents
        {
            get { return previousNumberOfIncidents; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Previous number of incidents cannot be negative.");
                }
                previousNumberOfIncidents = value;
            }
        }

        public int GetChange()
        {
            return CurrentNumberOfIncidents - PreviousNumberOfIncidents;
        }

        public Indicator(string indicatorName, int currentNumberOfIncidents, int previousNumberOfIncidents)
        {
            IndicatorName = indicatorName;
            CurrentNumberOfIncidents = currentNumberOfIncidents;
            PreviousNumberOfIncidents = previousNumberOfIncidents;
        }
    }

   
    // 5. INCIDENT HIERARCHY
   

    public abstract class Incident
    {
        private int incidentID;
        private string description;
        private int communityID;
        private DateTime date;
        private int impact;

        public abstract string GetDescription();

        public int IncidentID
        {
            get { return incidentID; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Incident ID cannot be negative.");
                }
                incidentID = value;
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Description cannot be empty.");
                }
                description = value;
            }
        }

        public int CommunityID
        {
            get { return communityID; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Community ID cannot be negative.");
                }
                communityID = value;
            }
        }

        public DateTime Date
        {
            get { return date; }
            set
            {
                if (value > DateTime.Now)
                {
                    throw new ArgumentException("Date cannot be in the future.");
                }
                date = value;
            }
        }

        public int Impact
        {
            get { return impact; }
            set
            {
                if (value < 0 || value > 100)
                {
                    throw new ArgumentException("Impact must be between 0 and 100.");
                }
                impact = value;
            }
        }
    }

    public class EmergencyIncident : Incident
    {
        public EmergencyIncident(int incidentID, string description, int communityID, DateTime date, int impact)
        {
            IncidentID = incidentID;
            Description = description;
            CommunityID = communityID;
            Date = date;
            Impact = impact;
        }

        public override string GetDescription()
        {
            return "Emergency incident";
        }
    }

    public class InfrastructureIncident : Incident
    {
        public InfrastructureIncident(int incidentID, string description, int communityID, DateTime date, int impact)
        {
            IncidentID = incidentID;
            Description = description;
            CommunityID = communityID;
            Date = date;
            Impact = impact;
        }

        public override string GetDescription()
        {
            return "Infrastructure problem";
        }
    }

    public class PowerIncident : Incident
    {
        public PowerIncident(int incidentID, string description, int communityID, DateTime date, int impact)
        {
            IncidentID = incidentID;
            Description = description;
            CommunityID = communityID;
            Date = date;
            Impact = impact;
        }

        public override string GetDescription()
        {
            return "Power outage";
        }
    }

    public class TransportIncident : Incident
    {
        public TransportIncident(int incidentID, string description, int communityID, DateTime date, int impact)
        {
            IncidentID = incidentID;
            Description = description;
            CommunityID = communityID;
            Date = date;
            Impact = impact;
        }

        public override string GetDescription()
        {
            return "Transport disruption";
        }
    }

    public class WasteIncident : Incident
    {
        public WasteIncident(int incidentID, string description, int communityID, DateTime date, int impact)
        {
            IncidentID = incidentID;
            Description = description;
            CommunityID = communityID;
            Date = date;
            Impact = impact;
        }

        public override string GetDescription()
        {
            return "Waste collection problem";
        }
    }

    public class WaterIncident : Incident
    {
        public WaterIncident()
        {
            IncidentID = 1;
            Description = "Water Supply Interruption";
            CommunityID = 1;
            Date = DateTime.Now;
            Impact = 50;
        }

        public WaterIncident(int incidentID, string description, int communityID, DateTime date, int impact)
        {
            IncidentID = incidentID;
            Description = description;
            CommunityID = communityID;
            Date = date;
            Impact = impact;
        }

        public override string GetDescription()
        {
            return "Water supply interruption";
        }
    }

   
    // 6. MANAGERS & ANALYZERS
   

    internal class AlertManager
    {
        public void HandleAnomalyDetected(object sender, AnomalyDetectedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("          ANOMALY DETECTED");
            Console.WriteLine("========================================");
            Console.WriteLine($"Community:       {e.CommunityName}");
            Console.WriteLine($"Indicator:       {e.IndicatorName}");
            Console.WriteLine($"Previous Value:  {e.PreviousValue}");
            Console.WriteLine($"Current Value:   {e.CurrentValue}");
            Console.WriteLine($"Percentage:      {e.PercentageChange:F1}%");
            Console.WriteLine("========================================");
        }

        public void HandlePulseLevelChanged(object sender, PulseLevelChangedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("         PULSE LEVEL CHANGED");
            Console.WriteLine("========================================");
            Console.WriteLine($"Community:   {e.CommunityName}");
            Console.WriteLine($"Previous:    {e.OldLevel}");
            Console.WriteLine($"New Level:   {e.NewLevel}");
            Console.WriteLine($"Pulse Score: {e.PulseScore}");
            Console.WriteLine("========================================");
        }
    }

    internal class CommunityManager : ICommunityManager, IInputValidator
    {
        private List<Community> communities = new List<Community>();

        public int GetValidInteger(string message)
        {
            int value;
            while (true)
            {
                Console.Write(message);
                if (int.TryParse(Console.ReadLine(), out value))
                {
                    return value;
                }
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }

        public int GetValidRating(string message)
        {
            int value;
            while (true)
            {
                Console.Write(message);
                if (int.TryParse(Console.ReadLine(), out value) && value >= 1 && value <= 10)
                {
                    return value;
                }
                Console.WriteLine("Please enter a number between 1 and 10.");
            }
        }

        public string GetValidText(string message)
        {
            while (true)
            {
                Console.Write(message);
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }
                Console.WriteLine("This field cannot be empty.");
            }
        }

        public void AddCommunity()
        {
            Console.WriteLine("\n===== ADD COMMUNITY =====");
            string name = GetValidText("Enter community name: ");
            int id = GetValidInteger("Enter community ID: ");

            while (communities.Any(c => c.CommunityID == id))
            {
                Console.WriteLine("That Community ID already exists.");
                id = GetValidInteger("Enter another Community ID: ");
            }

            int population = GetValidInteger("Enter population: ");
            while (population < 0)
            {
                Console.WriteLine("Population cannot be negative.");
                population = GetValidInteger("Enter population: ");
            }

            int pressure = GetValidRating("Enter population pressure (1-10): ");
            string riskLevel = GetValidText("Enter risk level: ");
            int pulseScore = GetValidInteger("Enter pulse score (0-100): ");

            while (pulseScore < 0 || pulseScore > 100)
            {
                Console.WriteLine("Pulse score must be between 0 and 100.");
                pulseScore = GetValidInteger("Enter pulse score (0-100): ");
            }

            try
            {
                Community community = new Community(name, id, population, pressure.ToString(), riskLevel, pulseScore);
                communities.Add(community);
                Console.WriteLine("Community added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding community: " + ex.Message);
            }
        }

        public void ViewCommunities()
        {
            Console.WriteLine("\n===== COMMUNITIES =====");
            if (communities.Count == 0)
            {
                Console.WriteLine("No communities found.");
                return;
            }

            foreach (Community community in communities)
            {
                Console.WriteLine($"ID: {community.CommunityID} | Name: {community.CommunityName} | Population: {community.Population} | Pulse Score: {community.PulseScore} | Status: {community.PulseStatus}");
            }
        }

        public void EditCommunity()
        {
            Console.WriteLine("\n===== EDIT COMMUNITY =====");
            if (communities.Count == 0)
            {
                Console.WriteLine("No communities available.");
                return;
            }

            int id = GetValidInteger("Enter Community ID to edit: ");
            Community community = communities.FirstOrDefault(c => c.CommunityID == id);

            if (community == null)
            {
                Console.WriteLine("Community not found.");
                return;
            }

            try
            {
                community.CommunityName = GetValidText("Enter new community name: ");
                int population = GetValidInteger("Enter new population: ");
                while (population < 0)
                {
                    Console.WriteLine("Population cannot be negative.");
                    population = GetValidInteger("Enter new population: ");
                }
                community.Population = population;
                community.PopulationPressure = GetValidRating("Enter new population pressure (1-10): ").ToString();
                community.RiskLevel = GetValidText("Enter new risk level: ");

                int pulseScore = GetValidInteger("Enter new pulse score (0-100): ");
                while (pulseScore < 0 || pulseScore > 100)
                {
                    Console.WriteLine("Pulse score must be between 0 and 100.");
                    pulseScore = GetValidInteger("Enter new pulse score (0-100): ");
                }
                community.PulseScore = pulseScore;

                Console.WriteLine("Community updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating community: " + ex.Message);
            }
        }

        public void DeleteCommunity()
        {
            Console.WriteLine("\n===== DELETE COMMUNITY =====");
            if (communities.Count == 0)
            {
                Console.WriteLine("No communities available.");
                return;
            }

            int id = GetValidInteger("Enter Community ID to delete: ");
            Community community = communities.FirstOrDefault(c => c.CommunityID == id);

            if (community == null)
            {
                Console.WriteLine("Community not found.");
                return;
            }

            communities.Remove(community);
            Console.WriteLine("Community deleted successfully.");
        }

        public void ViewCommunityDetails()
        {
            Console.WriteLine("\n===== COMMUNITY DETAILS =====");
            int id = GetValidInteger("Enter Community ID: ");
            Community community = communities.FirstOrDefault(c => c.CommunityID == id);

            if (community == null)
            {
                Console.WriteLine("Community not found.");
                return;
            }

            Console.WriteLine("Community: " + community.CommunityName);
            Console.WriteLine("ID: " + community.CommunityID);
            Console.WriteLine("Population: " + community.Population);
            Console.WriteLine("Population Pressure: " + community.PopulationPressure);
            Console.WriteLine("Risk Level: " + community.RiskLevel);
            Console.WriteLine("Pulse Score: " + community.PulseScore);
            Console.WriteLine("Pulse Status: " + community.PulseStatus);
        }
    }

    internal class CrossSignalAnalyzer
    {
        public event EventHandler<AnomalyDetectedEventArgs> AnomalyDetected;
        public event EventHandler<PulseLevelChangedEventArgs> PulseLevelChanged;

        private const double AnomalyThreshold = 30.0;
        private const int RequiredSignals = 2;

        public bool DetectCrossSignal(Community community, Indicator[] indicators)
        {
            ValidateCommunity(community);

            if (indicators == null || indicators.Length == 0)
            {
                throw new ArgumentException("At least one indicator is required.");
            }

            int significantSignals = 0;

            foreach (Indicator indicator in indicators)
            {
                if (indicator == null) continue;

                double percentageChange = CalculatePercentageChange(indicator.PreviousNumberOfIncidents, indicator.CurrentNumberOfIncidents);

                if (percentageChange >= AnomalyThreshold)
                {
                    significantSignals++;
                    OnAnomalyDetected(community, indicator);
                }
            }

            return significantSignals >= RequiredSignals;
        }

        private double CalculatePercentageChange(int previousValue, int currentValue)
        {
            if (previousValue == 0)
            {
                return currentValue > 0 ? 100.0 : 0.0;
            }

            return ((double)(currentValue - previousValue) / previousValue) * 100.0;
        }

        protected virtual void OnAnomalyDetected(Community community, Indicator indicator)
        {
            AnomalyDetected?.Invoke(this, new AnomalyDetectedEventArgs(
                community.CommunityName,
                indicator.IndicatorName,
                indicator.PreviousNumberOfIncidents,
                indicator.CurrentNumberOfIncidents));
        }

        public void CheckPulseLevelChange(Community community, int newPulseScore)
        {
            ValidateCommunity(community);

            if (newPulseScore < 0 || newPulseScore > 100)
            {
                throw new InvalidPulseScoreException("Pulse score must be between 0 and 100.");
            }

            string oldLevel = community.PulseStatus;
            community.PulseScore = newPulseScore;
            string newLevel = community.PulseStatus;

            if (!oldLevel.Equals(newLevel, StringComparison.OrdinalIgnoreCase))
            {
                OnPulseLevelChanged(community, oldLevel, newLevel, newPulseScore);
            }
        }

        protected virtual void OnPulseLevelChanged(Community community, string oldLevel, string newLevel, int pulseScore)
        {
            PulseLevelChanged?.Invoke(this, new PulseLevelChangedEventArgs(
                community.CommunityName,
                oldLevel,
                newLevel,
                pulseScore));
        }

        private void ValidateCommunity(Community community)
        {
            if (community == null)
            {
                throw new CommunityNotFoundException("The selected community could not be found.");
            }
        }
    }

   
    // 7. MULTITHREADING & SYSTEM PROCESSING
    

    public class IncidentProcessor
    {
        private readonly object queueLock = new object();
        private readonly Queue<Incident> queue = new Queue<Incident>();
        private volatile bool isRunning;
        private Thread workerThread;

        public void Start()
        {
            if (isRunning) return;
            isRunning = true;
            workerThread = new Thread(ProcessQueue) { IsBackground = true };
            workerThread.Start();
        }

        public void Stop()
        {
            if (!isRunning) return;
            isRunning = false;
            lock (queueLock)
            {
                Monitor.PulseAll(queueLock);
            }

            if (workerThread != null && !workerThread.Join(TimeSpan.FromSeconds(3)))
            {
                Console.WriteLine("[Processor] Background worker timeout cleanly handled.");
            }
        }

        public void AddIncident(Incident incident)
        {
            if (incident == null) throw new ArgumentNullException(nameof(incident));
            lock (queueLock)
            {
                queue.Enqueue(incident);
                Monitor.Pulse(queueLock);
            }
            Console.WriteLine("[Queue] Incident added to processing queue.");
        }

        private void ProcessQueue()
        {
            while (isRunning)
            {
                Incident currentIncident = null;

                lock (queueLock)
                {
                    if (queue.Count > 0)
                    {
                        currentIncident = queue.Dequeue();
                    }
                    else
                    {
                        Monitor.Wait(queueLock, 1000);
                    }
                }

                if (currentIncident != null)
                {
                    try
                    {
                        Console.WriteLine("[Worker] Processing incident...");
                        Thread.Sleep(1000);
                        string logText = $"[{DateTime.Now}] Processed: {currentIncident.GetType().Name}{Environment.NewLine}";
                        File.AppendAllText("incident_log.txt", logText);
                        Console.WriteLine("[Worker] Incident resolved and logged to file.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing incident: " + ex.Message);
                    }
                }
            }
        }
    }

    public class SystemMonitor
    {
        private volatile bool isRunning;
        private readonly ManualResetEventSlim stopSignal = new ManualResetEventSlim(false);
        private int checksPerformed;

        public event Action MonitorStopped;

        public bool IsRunning => isRunning;

        public void Start(int maxChecks = 5)
        {
            if (isRunning)
            {
                Console.WriteLine("System monitor is already running.");
                return;
            }

            checksPerformed = 0;
            isRunning = true;
            stopSignal.Reset();

            Thread monitorThread = new Thread(() => MonitorLoop(maxChecks)) { IsBackground = true };
            monitorThread.Start();

            Console.WriteLine($"System monitor started (will check up to {maxChecks} times).");
        }

        public void Stop()
        {
            if (!isRunning)
            {
                Console.WriteLine("System monitor is not running.");
                return;
            }

            isRunning = false;
            stopSignal.Set();
            Console.WriteLine("System monitor stopped.");
            MonitorStopped?.Invoke();
        }

        private void MonitorLoop(int maxChecks)
        {
            try
            {
                while (isRunning)
                {
                    checksPerformed++;
                    Console.WriteLine($"\n[System Check #{checksPerformed}] Status: OK | Time: {DateTime.Now:HH:mm:ss}");

                    if (maxChecks > 0 && checksPerformed >= maxChecks)
                    {
                        isRunning = false;
                        Console.WriteLine("[System Monitor] Completed target checks. Stopping monitor automatically.");
                        MonitorStopped?.Invoke();
                        break;
                    }

                    if (stopSignal.Wait(3000))
                    {
                        break;
                    }
                }
            }
            finally
            {
                isRunning = false;
                stopSignal.Reset();
            }
        }
    }

 
    // 8. ENTRY POINT (MAIN)
   

    internal class Program
    {
        private static readonly CommunityManager communityManager = new CommunityManager();
        private static readonly CrossSignalAnalyzer analyzer = new CrossSignalAnalyzer();
        private static readonly AlertManager alertManager = new AlertManager();
        private static readonly SystemMonitor monitor = new SystemMonitor();
        private static readonly IncidentProcessor processor = new IncidentProcessor();

        private enum MainMenuOption
        {
            ManageCommunities = 1,
            AddWaterIncident = 2,
            RunAnalyzerTests = 3,
            ToggleMonitoring = 4,
            Exit = 9
        }

        private enum CommunityMenuOption
        {
            Add = 1,
            View = 2,
            Edit = 3,
            Delete = 4,
            Details = 5,
            Back = 9
        }

        static void Main(string[] args)
        {
            // Subscribe events
            analyzer.AnomalyDetected += alertManager.HandleAnomalyDetected;
            analyzer.PulseLevelChanged += alertManager.HandlePulseLevelChanged;

            // Start processor queue
            processor.Start();

            RunMainMenuLoop();

            // Clean shutdown
            if (monitor.IsRunning) monitor.Stop();
            processor.Stop();
            Console.WriteLine("System shut down cleanly.");
        }

        private static void RunMainMenuLoop()
        {
            bool running = true;

            while (running)
            {
                DisplayMainMenu();
                MainMenuOption choice = ReadMenuChoice<MainMenuOption>();

                switch (choice)
                {
                    case MainMenuOption.ManageCommunities:
                        RunCommunityMenu();
                        break;

                    case MainMenuOption.AddWaterIncident:
                        processor.AddIncident(new WaterIncident());
                        break;

                    case MainMenuOption.RunAnalyzerTests:
                        RunAnalyzerTests();
                        break;

                    case MainMenuOption.ToggleMonitoring:
                        if (!monitor.IsRunning)
                        {
                            monitor.Start(maxChecks: 5); // Automatically stops after 5 checks so menu stays clean
                        }
                        else
                        {
                            monitor.Stop();
                        }
                        break;

                    case MainMenuOption.Exit:
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Unknown option. Try again.");
                        break;
                }
            }
        }

        private static void DisplayMainMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== MAIN MENU =====");
            Console.WriteLine("1 - Manage Communities");
            Console.WriteLine("2 - Add Water Incident");
            Console.WriteLine("3 - Run Analyzer Tests");
            Console.WriteLine("4 - Start/Stop System Monitor");
            Console.WriteLine("9 - Exit");
            Console.Write("Select option: ");
        }

        private static T ReadMenuChoice<T>() where T : struct
        {
            string input = Console.ReadLine();
            if (int.TryParse(input, out int val))
            {
                if (Enum.IsDefined(typeof(T), val))
                {
                    return (T)Enum.ToObject(typeof(T), val);
                }
            }
            return default(T);
        }

        private static void RunCommunityMenu()
        {
            bool back = false;
            while (!back)
            {
                DisplayCommunityMenu();
                CommunityMenuOption choice = ReadMenuChoice<CommunityMenuOption>();

                switch (choice)
                {
                    case CommunityMenuOption.Add:
                        communityManager.AddCommunity();
                        break;

                    case CommunityMenuOption.View:
                        communityManager.ViewCommunities();
                        break;

                    case CommunityMenuOption.Edit:
                        communityManager.EditCommunity();
                        break;

                    case CommunityMenuOption.Delete:
                        communityManager.DeleteCommunity();
                        break;

                    case CommunityMenuOption.Details:
                        communityManager.ViewCommunityDetails();
                        break;

                    case CommunityMenuOption.Back:
                        back = true;
                        break;

                    default:
                        Console.WriteLine("Unknown option. Try again.");
                        break;
                }
            }
        }

        private static void DisplayCommunityMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== COMMUNITY MENU =====");
            Console.WriteLine("1 - Add Community");
            Console.WriteLine("2 - View All Communities");
            Console.WriteLine("3 - Edit Community");
            Console.WriteLine("4 - Delete Community");
            Console.WriteLine("5 - View Community Details");
            Console.WriteLine("9 - Back");
            Console.Write("Select option: ");
        }

        private static void RunAnalyzerTests()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("        SA PULSE - ANALYZER ");
                Console.WriteLine("========================================");

                Community demoCommunity = new Community("DemoTown", 100, 1000, "Medium", "Low", 45);

                Indicator water = new Indicator("Water", 35, 20);
                Indicator electricity = new Indicator("Electricity", 50, 25);
                Indicator transport = new Indicator("Transport", 70, 40);

                Indicator[] indicators = { water, electricity, transport };

                Console.WriteLine("TEST: Cross-Signal Detection");
                bool crossSignal = analyzer.DetectCrossSignal(demoCommunity, indicators);
                Console.WriteLine(crossSignal ? "RESULT: Cross-signal anomaly detected." : "RESULT: No cross-signal anomaly detected.");

                Console.WriteLine();
                Console.WriteLine("TEST: Pulse Level Change (setting score to 75)");
                analyzer.CheckPulseLevelChange(demoCommunity, 75);

                Console.WriteLine();
                Console.WriteLine("TEST: Invalid Pulse Score Exception (setting score to 150)");
                analyzer.CheckPulseLevelChange(demoCommunity, 150);
            }
            catch (InvalidPulseScoreException ex)
            {
                Console.WriteLine("CUSTOM EXCEPTION CAUGHT SUCCESSFULLY:");
                Console.WriteLine(ex.Message);
            }
            catch (CommunityNotFoundException ex)
            {
                Console.WriteLine("COMMUNITY ERROR:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UNEXPECTED ERROR:");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("========================================");
                Console.WriteLine("        ANALYZER FINISHED");
                Console.WriteLine("========================================");
            }
        }
    }
}