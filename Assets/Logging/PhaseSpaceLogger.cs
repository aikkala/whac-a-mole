using PhaseSpace.Unity;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using Debug = UnityEngine.Debug;
using ThreadPriority = System.Threading.ThreadPriority;


/// <summary>
/// Represents a logging interface for Phasespace data.
/// </summary>
public class PhaseSpaceLogger : Logger {

    private CancellationTokenSource cancellation;
    private LogData logData;
    private Thread logThread;
    private ConcurrentQueue<(string timestamp, Marker marker)> dataQueue = new ConcurrentQueue<(string, Marker)>();



    /// <summary>
    /// Will be called when the component gets enabled.
    /// </summary>
    private void OnEnable() {
        try {
            var path = LogUtils.GenerateLogFileName("PhaseSpace");
            var log = new CSVWriter(path);
            log.WriteRow("Timestamp", "Id", "Marker Time", "Condition", "Position.x", "Position.y", "Position.z");

            this.cancellation = new CancellationTokenSource();
            this.logData = new LogData() {
                cancellation = this.cancellation.Token,
                log = log,
                //phaseSpace = App.Instance.PhaseSpaceClient
            };

            this.dataQueue = new ConcurrentQueue<(string, Marker)>();
            //App.Instance.PhaseSpaceManager.OnMarkerFrame.AddListener(this.LogMarker);

            this.logThread = new Thread(e => {
                var data = ( LogData ) e;

                try {
                    while ( !data.cancellation.IsCancellationRequested ) {
                        for (int i=0; i<10; i++) {
                            if ( this.dataQueue.TryDequeue(out var dataItem) ) {
                                if ( dataItem.marker.Condition <= TrackingCondition.Invalid ) continue;
                                data.log.WriteRow(
                                    dataItem.timestamp,
                                    dataItem.marker.id,
                                    dataItem.marker.time,
                                    dataItem.marker.Condition,
                                    dataItem.marker.position.x,
                                    dataItem.marker.position.y,
                                    dataItem.marker.position.z
                                );
                            }
                        }

                        Thread.Sleep(0);
                    }
                }
                catch ( ThreadInterruptedException ) { }
                finally {
                    while ( this.dataQueue.TryDequeue(out var dataItem) ) {
                        if ( dataItem.marker.Condition <= TrackingCondition.Invalid ) continue;
                        data.log.WriteRow(
                            dataItem.timestamp,
                            dataItem.marker.id,
                            dataItem.marker.time,
                            dataItem.marker.Condition,
                            dataItem.marker.position.x,
                            dataItem.marker.position.y,
                            dataItem.marker.position.z
                        );
                    }
                }
            }) { IsBackground = true, Priority = ThreadPriority.AboveNormal };
            this.logThread.Start(this.logData);
        }
        catch ( Exception ex ) {
            Debug.LogException(ex);
            this.enabled = false;
        }
    }

    /// <summary>
    /// Will be called when the componen gets disabled.
    /// </summary>
    private void OnDisable() {
        //App.Instance.PhaseSpaceManager.OnMarkerFrame.RemoveListener(this.LogMarker);
        this.cancellation?.Cancel();
        this.logThread?.Interrupt();
        this.logThread?.Join();
        this.logData?.Dispose();
    }



    /// <summary>
    /// Logs the given marker information.
    /// </summary>
    /// <param name="timestamp">The timestamp of the marker information.</param>
    /// <param name="marker">The marker information to log.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogMarker(PhaseSpaceMarkerEventArgs e) {
        if ( this.cancellation?.IsCancellationRequested ?? false ) return;
        this.dataQueue.Enqueue((e.Timestamp, e.Marker));
    }



    /// <summary>
    /// Provides information for a Phasespace logging.
    /// </summary>
    private sealed class LogData : IDisposable {

        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationToken cancellation;
        /// <summary>
        /// The log to write to.
        /// </summary>
        public CSVWriter log;
        /// <summary>
        /// The Phasespace client to read data from.
        /// </summary>
        public OWLClient phaseSpace;



        /// <summary>
        /// Disposes all resources used by the instance.
        /// </summary>
        public void Dispose() {
            this.log?.Dispose();

            this.log = null;
            this.phaseSpace = null;
        }

    }

}