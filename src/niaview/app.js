
activityData = null
defaultGraphOptions = {
  responsive: true,
  // scrollZoom: true, // lets us scroll to zoom in and out - works
  showLink: false, // removes the link to edit on plotly - works
  // modeBarButtonsToRemove: ['toImage', 'zoom2d', 'pan', 'pan2d', 'autoScale2d']
}
defaultLayout = {
  // bgcolor: '#111',
  // paper_bgcolor: '#111',
  // plot_bgcolor: '#111'
}

// https://stackoverflow.com/a/14991797
function parseCSV(str) {
  var arr = [];
  var quote = false;
  for (var row = col = c = 0; c < str.length; c++) {
    var cc = str[c], nc = str[c + 1];
    arr[row] = arr[row] || [];
    arr[row][col] = arr[row][col] || '';

    if (cc == '"' && quote && nc == '"') { arr[row][col] += cc; ++c; continue; }
    if (cc == '"') { quote = !quote; continue; }
    if (cc == ',' && !quote) { ++col; continue; }
    if (cc == '\n' && !quote) { ++row; col = 0; continue; }

    arr[row][col] += cc;
  }
  return arr;
}

// https://stackoverflow.com/a/19700358
function msToTime(duration) {
  var milliseconds = parseInt((duration % 1000) / 100),
    seconds = parseInt((duration / 1000) % 60),
    minutes = parseInt((duration / (1000 * 60)) % 60),
    hours = parseInt((duration / (1000 * 60 * 60)) % 24)
    days = parseInt((duration / (1000 * 60 * 60 * 24)));

  days = (days < 10) ? "0" + days : days;
  hours = (hours < 10) ? "0" + hours : hours;
  minutes = (minutes < 10) ? "0" + minutes : minutes;
  seconds = (seconds < 10) ? "0" + seconds : seconds;

  return days + ":" + hours + ":" + minutes + ":" + seconds + "." + milliseconds;
}

function onActivityUploaded(files) {
  var file = files[0];
  var reader = new FileReader();
  reader.readAsText(file)
  reader.onload = () => {
    activityData = parseCSV(reader.result)
    graphActivityApplications()
    graphActivityTimeline()
  }
}

function convertTimestamp(uts) {
  var now = new Date(uts);
  return now;
}

function graphActivityTimeline() {
  var element = document.getElementById('plot_timeline');

  var data = []

  var timeline = {
    x: [],
    y: [],
    type: 'scatter',
    opacity: 0.5,
    line: { width: 20 },
    mode: 'markers',
    marker: {
      symbol: 'circle',
      size: [],
      // sizemin: 4,
      // sizemax: 20,
      // sizemode: 'area'
    },
    text: [],
    name: 'usage'
  }
  for (var i = 1; i < activityData.length; i++) {
    var row = activityData[i];
    var application = row[1]
    var title = row[2]
    var start = parseInt(row[6])
    var duration = parseInt(row[7])
    var kbCount = parseInt(row[8])
    var startTs = convertTimestamp(start)
    var endTs = convertTimestamp(start + duration)

    // upgrade old DB schema
    if (kbCount == 0) kbCount = 1

    var kbRate = kbCount / (duration * 1000)
    // TODO: consider using kbRate instead
    timeline.x.push(startTs)
    timeline.y.push(duration / 1000)
    var humanTime = msToTime(duration)
    timeline.text.push(`(${humanTime}/${kbCount}) - ${application} - ${title}`)
    // transform kbCount
    KBCOUNT_MAX = 1000
    var markerSize = kbCount
    if (markerSize > KBCOUNT_MAX) markerSize = KBCOUNT_MAX
    markerSize = Math.floor(4 + 16 * (markerSize / KBCOUNT_MAX))
    timeline.marker.size.push(markerSize)
  }
  data.push(timeline)

  var layout = {
    title: 'Session Timeline',
    xaxis: {
      title: 'Time'
    },
    yaxis: {
      title: 'Time (s)'
    }
  }
  Plotly.newPlot(element, data, { ...defaultLayout, ...layout }, { ...defaultGraphOptions });
}

function graphActivityApplications() {
  var element = document.getElementById('plot_applications');

  var data = []

  var apptime = {}
  var totalTime = 0
  for (var i = 1; i < activityData.length; i++) {
    var row = activityData[i];
    var application = row[1]
    var start = parseInt(row[6])
    var duration = parseInt(row[7])
    if (!(application in apptime)) {
      apptime[application] = 0
    }
    apptime[application] += duration
    totalTime += duration
  }
  var fTotalTime = msToTime(totalTime)

  var pie = {
    values: [],
    labels: [],
    type: 'pie',
    hole: 0.2
  }

  for (var app in apptime) {
    pie.values.push(apptime[app])
    ftime = msToTime(apptime[app])
    pie.labels.push(`${app} - ${ftime}`)
  }
  console.log(pie)
  data.push(pie)

  var layout = {
    title: `Application Usage (${fTotalTime})`
  }
  Plotly.newPlot(element, data, { ...defaultLayout, ...layout }, { ...defaultGraphOptions });
}
