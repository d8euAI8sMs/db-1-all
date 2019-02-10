using FirstFloor.ModernUI.Presentation;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Management;	

namespace PhoneBook
{

    class MainModel : NotifyPropertyChanged
    {

        public const string TablePostfix = "_Vasilevsky_Alexander";
        public const string ProcessTable = "Process" + TablePostfix;
        public const string UserTable = "User" + TablePostfix;
        public const string SessionTable = "Session" + TablePostfix;

        public static Monitor Monitor = new Monitor();
        public static MainModel Instance = new MainModel();

        public Monitor.Callback StartCallback;
        public Monitor.Callback ExitCallback;

        public MainModel()
        {
            Monitor.UserCallback += ProcessCallback;
        }

        private void ProcessCallback(System.Diagnostics.Process p, string username)
        {
            DateTime startTime = DateTime.Now;
            try
            {
                startTime = p.StartTime;
            }
            catch (Exception ex) { }
            Event evt;
            try
            {
                evt = new Event(p.ProcessName, username, p.Id, startTime, true);
                p.EnableRaisingEvents = true;
            }
            catch (Exception ex) { return; }
            try
            {
                p.Exited += ProcessExited(p, username, ProcessExitedCallback);
            }
            catch (Exception ex) { return; }
            Persist(evt);
            if (StartCallback != null) StartCallback(p, username);
        }

        private EventHandler ProcessExited(
            System.Diagnostics.Process p, string username, Monitor.Callback callback)
        {
            return (object sender, EventArgs args) => {
                callback(p, username);
            };
        }

        private void ProcessExitedCallback(System.Diagnostics.Process p, string username)
        {
            DateTime exitTime = DateTime.Now;
            try
            {
                exitTime = p.ExitTime;
            }
            catch (Exception ex) { }
            Event evt = new Event(p.ProcessName, username, p.Id, exitTime, false);
            Persist(evt);
            if (ExitCallback != null) ExitCallback(p, username);
        }

        public string Connection { get; set; }

        public ICollection<ProcessSummary> GetSummary(
            string processPattern = "",
            string userPattern = "",
            DateTime? start = null, DateTime? end = null)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                DataTable data = new DataTable();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT [ProcessId], [ProcessName],
                      COUNT([EndTime]) as [Count],
                      SUM(DATEDIFF(second, [StartTime], [EndTime])) as [Total],
                      AVG(DATEDIFF(second, [StartTime], [EndTime])) as [Average],
                      MIN([StartTime]) as [Min],
                      MAX([StartTime]) as [Max]
                      FROM [" + SessionTable + @"] INNER JOIN [" + ProcessTable
                    + @"] ON [Process] = [ProcessId] INNER JOIN [" + UserTable
                    + @"] ON [User] = [UserId] ", c);
                if (!String.IsNullOrWhiteSpace(processPattern)
                    || !String.IsNullOrWhiteSpace(userPattern)
                    || start != null || end != null) cmd.CommandText += " WHERE ";
                bool and = false;
                if (!String.IsNullOrWhiteSpace(processPattern))
                {
                    cmd.CommandText += @" [ProcessName] LIKE @Process ";
                    cmd.Parameters.Add("@Process", processPattern);
                    and = true;
                }
                if (!String.IsNullOrWhiteSpace(userPattern))
                {
                    cmd.CommandText += (and ? " AND " : "") + @" [UserName] LIKE @User ";
                    cmd.Parameters.Add("@User", userPattern);
                    and = true;
                }
                if (start != null)
                {
                    cmd.CommandText += (and ? " AND " : "") + @" [StartTime] >= @Start ";
                    cmd.Parameters.Add("@Start", start);
                    and = true;
                }
                if (end != null)
                {
                    cmd.CommandText += (and ? " AND " : "") + @" [EndTime] < @End ";
                    cmd.Parameters.Add("@End", end);
                }
                cmd.CommandText += @" GROUP BY [ProcessId], [ProcessName]";
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(data);
                Collection<ProcessSummary> result = new Collection<ProcessSummary>();
                try
                {

                foreach (DataRow r in data.AsEnumerable())
                {
                    Process p = new Process((int)r.ItemArray[0], (string)r.ItemArray[1]);
                    result.Add(new ProcessSummary(
                        p,
                        (int) r.ItemArray[2],
                        r.ItemArray[3] == DBNull.Value ? 0 : (int)r.ItemArray[3],
                        r.ItemArray[4] == DBNull.Value ? 0 : (int)r.ItemArray[4],
                        (DateTime) r.ItemArray[5],
                        (DateTime) r.ItemArray[6])
                    );
                }
                } catch (Exception ex)
                {
                    return null;
                }
                return result;
            }
        }
        public ICollection<Session> GetSessions(
            string processPattern = "",
            string userPattern = "",
            DateTime? start = null, DateTime? end = null)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                DataTable data = new DataTable();
                SqlCommand cmd = new SqlCommand(
                     @"SELECT [ProcessId], [ProcessName], [UserId], [UserName],
                      [SessionId], [Pid], [StartTime], [EndTime]
                      FROM [" + SessionTable + @"] INNER JOIN [" + ProcessTable
                    + @"] ON [Process] = [ProcessId] INNER JOIN " + UserTable + @"
                      ON [User] = [UserId] WHERE [EndTime] IS NOT NULL ", c);
                if (!String.IsNullOrWhiteSpace(processPattern))
                {
                    cmd.CommandText += @" AND [ProcessName] LIKE @Process ";
                    cmd.Parameters.Add("@Process", processPattern);
                }
                if (!String.IsNullOrWhiteSpace(userPattern))
                {
                    cmd.CommandText += @" AND [UserName] LIKE @User ";
                    cmd.Parameters.Add("@User", userPattern);
                }
                if (start != null)
                {
                    cmd.CommandText += @" AND [StartTime] >= Convert(datetime, @Start) ";
                    cmd.Parameters.Add("@Start", start);
                }
                if (end != null)
                {
                    cmd.CommandText += @" AND [EndTime] < Convert(datetime, @End) ";
                    cmd.Parameters.Add("@End", end);
                }
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(data);
                Collection<Session> result = new Collection<Session>();
                foreach (DataRow r in data.AsEnumerable())
                {
                    Process p = new Process((int)r.ItemArray[0], (string)r.ItemArray[1]);
                    User u = new User((int)r.ItemArray[2], (string)r.ItemArray[3]);
                    result.Add(new Session(
                        (int)r.ItemArray[4],
                        p, u,
                        (int)r.ItemArray[5],
                        (DateTime)r.ItemArray[6],
                        (DateTime?)(r.ItemArray[7] == DBNull.Value ? null : r.ItemArray[7]))
                    );
                }
                return result;
            }
        }

        public Process GetOrPersistProcess(string name, SqlConnection c)
        {
            SqlCommand cmd = new SqlCommand(
                @"SELECT [ProcessId] FROM [" + ProcessTable + @"] 
                WHERE [ProcessName] = @Name", c
            );
            cmd.Parameters.Add("@Name", name);
            object result = cmd.ExecuteScalar();
            if (result == DBNull.Value || result == null)
            {
                cmd.CommandText = 
                @"INSERT INTO [" + ProcessTable + @"] 
                ([ProcessName]) VALUES (@Name)";
                cmd.ExecuteNonQuery();
                return GetOrPersistProcess(name, c);
            }
            return new Process((int)result, name);
        }

        public User GetOrPersistUser(string name, SqlConnection c)
        {
            SqlCommand cmd = new SqlCommand(
                @"SELECT [UserId] FROM [" + UserTable + @"] 
                WHERE [UserName] = @Name", c
            );
            cmd.Parameters.Add("@Name", name);
            object result = cmd.ExecuteScalar();
            if (result == DBNull.Value || result == null)
            {
                cmd.CommandText =
                @"INSERT INTO [" + UserTable + @"] 
                ([UserName]) VALUES (@Name)";
                cmd.ExecuteNonQuery();
                return GetOrPersistUser(name, c);
            }
            return new User((int)result, name);
        }

        public void Persist(Event evt)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                c.Open();

                Process p = GetOrPersistProcess(evt.ProcessName, c);
                User u = GetOrPersistUser(evt.UserName, c);

                if (!evt.IsStartup)
                {
                    SqlCommand cmd = new SqlCommand(
                        "SELECT MAX([StartTime]) FROM " + SessionTable + " WHERE " +
                        " [Process] = @Process AND [User] = @User AND [Pid] = @Pid", c);
                    cmd.Parameters.Add(new SqlParameter("@Process", p.Id));
                    cmd.Parameters.Add(new SqlParameter("@User", u.Id));
                    cmd.Parameters.Add(new SqlParameter("@Pid", evt.Pid));
                    cmd.Parameters.Add(new SqlParameter("@Time", evt.Time));
                    object result = cmd.ExecuteScalar();
                    if (result == DBNull.Value || result == null) return;
                    cmd.CommandText = "UPDATE " + SessionTable + " SET "
                        + " [EndTime] = @Time "
                        + " WHERE [Process] = @Process AND [User] = @User AND [Pid] = @Pid AND [StartTime] = @MaxST";
                    cmd.Parameters.Add(new SqlParameter("@MaxST", (DateTime) result));
                    cmd.ExecuteNonQuery();
                    return;
                }
                
                SqlCommand cmd2 = new SqlCommand(
                        "INSERT INTO " + SessionTable + " ([Process], [Pid], [User], [StartTime]) " +
                        " VALUES (@Process, @Pid, @User, @Time)", c);
                cmd2.Parameters.Add(new SqlParameter("@Process", p.Id));
                cmd2.Parameters.Add(new SqlParameter("@Pid", evt.Pid));
                cmd2.Parameters.Add(new SqlParameter("@User", u.Id));
                cmd2.Parameters.Add(new SqlParameter("@Time", evt.Time));
                cmd2.ExecuteNonQuery();
            }
        }

        public void RunSql(string sql)
        {
            using (SqlConnection c = new SqlConnection(Connection))
            {
                c.Open();
                String pattern = @"^GO";
                String[] elements = Regex.Split(sql, pattern, RegexOptions.Multiline);
                string message = "";
                foreach (var command in elements)
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(command, c);
                        cmd.ExecuteNonQuery();
                    } catch (Exception ex)
                    {
                        message += ex.Message + "\n";
                    }
                }
                if (!String.IsNullOrWhiteSpace(message))
                {
                    throw new Exception(message);
                }
            }
        }
    }

    public class Entity
    {
        public int Id { get; set; }
        public override bool Equals(object other)
        {
            return (other is Entity) && (other as Entity).Id == Id;
        }
        public override int GetHashCode()
        {
            return this.Id;
        }
    }


    public class Process : Entity
    {
        public Process(int id, string name)
        {
            Id = id;
            ProcessName = name;
        }
        public Process(Process other) : this(other.Id, other.ProcessName)
        {
        }
        public string ProcessName { get; set; }
    }
    public class User : Entity
    {
        public User(int id, string name)
        {
            Id = id;
            UserName = name;
        }
        public User(User other) : this(other.Id, other.UserName)
        {
        }
        public string UserName { get; set; }
    }

    public class Session : Entity
    {
        public Session(int id, Process process, User user, int pid, DateTime start, DateTime? end)
        {
            Id = id;
            Process = process;
            User = user;
            StartTime = start;
            EndTime = end;
            Pid = pid;
        }
        public Process Process { get; set; }
        public User User { get; set; }
        public int Pid { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class ProcessSummary
    {
        public ProcessSummary(
            Process process,
            int count,
            int totalTime, int avgTime,
            DateTime firstStartup, DateTime lastStartup)
        {
            Process = process;
            Count = count;
            TotalTime = totalTime;
            AverageTime = avgTime;
            FirstStartupTime = firstStartup;
            LastStartupTime = lastStartup;
        }
        public Process Process { get; set; }
        public int Count { get; set; }
        public int TotalTime { get; set; }
        public int AverageTime { get; set; }
        public DateTime FirstStartupTime { get; set; }
        public DateTime LastStartupTime { get; set; }
    }

    public class Event
    {
        public Event(
            string processName,
            string userName,
            int pid,
            DateTime time,
            bool isStartup)
        {
            ProcessName = processName;
            UserName = userName;
            IsStartup = isStartup;
            Time = time;
            Pid = pid;
        }
        public int Pid { get; set; }
        public string ProcessName { get; set; }
        public string UserName { get; set; }
        public bool IsStartup { get; set; }
        public DateTime Time { get; set; }
    }

    public class Monitor
    {

        private static System.Threading.Thread t;

        public static Callback UserCallback;

        static Monitor()
        {
            t = new System.Threading.Thread(ThreadStart);
            t.IsBackground = true;
            t.Start();
        }

        private static volatile System.Diagnostics.Process[] old;

        public delegate void Callback(System.Diagnostics.Process p, string user);
        
        static void ThreadStart()
        {
            while (true)
            {
                System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcesses();
                if (old != null && old.Length != 0)
                {
                    var @new = procs.Except(old, new ProcessEqualityComparer());
                        //.Union(old.Except(procs, new ProcessEqualityComparer()));
                    foreach (var e in @new)
                    {
                        string username = null;
                        var mos = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ProcessId = " + e.Id);
                        foreach (var obj in mos.Get())
                        {
                            string[] args = new string[2];

                            object outParams =
                                (obj as ManagementObject).InvokeMethod("GetOwner",
                                (object[])args);

                            username = args[1] + @"\" + args[0];
                        }
                        if (UserCallback != null)
                            UserCallback(e, username);
                    }
                }
                old = procs;
                System.Threading.Thread.Sleep(1000);
            }
        }
    }

    public class ProcessEqualityComparer : IEqualityComparer<System.Diagnostics.Process>
    {
        public bool Equals(System.Diagnostics.Process b1, System.Diagnostics.Process b2)
        {
            if (b2 == null && b1 == null)
                return true;
            else if (b1 == null | b2 == null)
                return false;
            else if (b1.Id == b2.Id)
                return true;
            else
                return false;
        }

        public int GetHashCode(System.Diagnostics.Process bx)
        {
            return bx.Id;
        }
    }
}
