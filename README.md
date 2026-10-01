SA Pulse - Community Monitoring & Incident Management System
Course: PRG281 / Final Project

Language: C# (.NET Console Application)

Architecture: Object-Oriented, Event-Driven, Multi-threaded

====================================================================

SYSTEM OVERVIEW

SA Pulse is an enterprise-grade community monitoring and incident response console application. The system tracks community health metrics--such as Pulse Score, Population Pressure, and Risk Levels--and correlates incoming infrastructure incidents (water, power, transport, waste, and emergency events) in real time.

The application leverages an Event-Driven Publisher-Subscriber model to detect statistical anomalies and a Multi-threaded Producer-Consumer pipeline to process background operations asynchronously without interrupting user interactions.

====================================================================

EXECUTION INSTRUCTIONS

Prerequisites:

.NET SDK 6.0 / 7.0 / 8.0 or higher

Visual Studio 2022, JetBrains Rider, or Visual Studio Code

Build and Run via .NET CLI:

Open a terminal or command prompt and navigate to the project directory:
cd Final281_Project_Prg

Build the solution:
dotnet build

Execute the application:
dotnet run

====================================================================

ARCHITECTURAL DESIGN & TECHNICAL CONCEPTS

Object-Oriented Programming (OOP):

Encapsulation: Backing fields across domain models (Community, Indicator, Incident) are declared strictly as private. State mutations pass through properties containing business validation rules (e.g., rejecting negative population values or out-of-bounds pulse scores).

Abstraction: The base class Incident exposes an abstract string GetDescription() method, enforcing a contract that forces derived classes to supply domain-specific details.

Inheritance & Polymorphism: Subclasses (EmergencyIncident, WaterIncident, PowerIncident, TransportIncident, WasteIncident, InfrastructureIncident) inherit common fields from Incident while providing specialized polymorphic implementations for description evaluation.

Interfaces & System Decoupling:

ICommunityManager: Defines the lifecycle contract for managing community entities independently of UI execution.

IInputValidator: Establishes a standard contract for console input parsing and validation logic.

Exception Handling & Reliability:

Custom Exceptions: Domain-specific exceptions (CommunityNotFoundException, InvalidPulseScoreException) enforce logical domain constraints.

Fault Tolerance: Console interactions wrap operations inside try-catch-finally blocks to intercept illegal operations gracefully without crashing the execution process.

====================================================================

MULTITHREADING & EVENT-DRIVEN ARCHITECTURE

Publisher-Subscriber Event Architecture:
The core domain uses strongly typed EventArgs subclasses to notify registered subscribers of critical state shifts:

AnomalyDetected: Emitted by CrossSignalAnalyzer when indicator metrics breach configured operational thresholds (>= 30%).

PulseLevelChanged: Triggered when a community's status transitions between risk bands (Normal, Watch, Elevated, High, Critical).

AlertManager serves as the event subscriber, capturing dispatched notifications and writing structured output to the console.

Concurrency & Thread Safety:
To model concurrent, real-time background execution, two dedicated threading models are implemented:

IncidentProcessor (Producer-Consumer Processing Queue):

Runs an isolated background thread (IsBackground = true) executing ProcessQueue().

Implements thread synchronization using lock (queueLock) alongside Monitor.Wait and Monitor.Pulse to safely block and resume background processing when new incidents enter the queue.

SystemMonitor (Periodic Background Health Check):

Performs periodic health audits on an independent worker thread.

Employs ManualResetEventSlim for cooperative thread synchronization and clean shutdown signaling.

====================================================================

SECONDARY / BONUS TECHNICAL FEATURES

Persistent File Logging: Processed incidents are written asynchronously to an append-only log file on disk (incident_log.txt).

LINQ Queries: Utilizes LINQ extension methods (.Any() and .FirstOrDefault()) within CommunityManager for efficient entity lookup and duplicate validation.

====================================================================

PROJECT DIRECTORY STRUCTURE

Final281_Project_Prg/
|-- Interfaces/
|   |-- ICommunityManager.cs
|   |-- IInputValidator.cs
|
|-- Models & Domain/
|   |-- Community.cs
|   |-- Indicator.cs
|   |-- Incidents/
|
|-- Events & Services/
|   |-- EventArguments.cs
|   |-- CrossSignalAnalyzer.cs
|   |-- AlertManager.cs
|
|-- Concurrency/
|   |-- IncidentProcessor.cs
|   |-- SystemMonitor.cs
|
|-- Exceptions/
|   |-- CommunityNotFoundException.cs
|   |-- InvalidPulseScoreException.cs
|
|-- Program.cs
|-- README.md