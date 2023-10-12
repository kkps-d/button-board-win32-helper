class SessionElement {
  /**
   *
   * @param {any} socket
   * @param {{
   * sessionId: string,
   * friendlyName: string,
   * iconPath: string | null,
   * volumePercent: number,
   * muted: boolean
   * }} session
   */
  constructor(socket, session) {
    for (let [key, value] of Object.entries(session)) {
      this[key] = value;
    }

    console.log(session);

    this.socket = socket;

    this.sessionBarDiv = document.createElement("div");
    this.sessionBarDiv.classList.add("session", "bar");

    this.iconDiv = document.createElement("div");
    this.iconDiv.classList.add("icon");

    this.iconImg = document.createElement("img");
    this.iconImg.src = `/icons/${session.sessionId}.png`;
    this.iconDiv.appendChild(this.iconImg);

    this.labelSpan = document.createElement("span");
    this.labelSpan.classList.add("label");
    this.labelSpan.innerText = this.friendlyName;

    let volRangeDiv = document.createElement("div");
    volRangeDiv.classList.add("vol-range");

    this.muteButton = document.createElement("button");
    this.muteButton.innerText = "🔊";
    volRangeDiv.appendChild(this.muteButton);

    let rangeDiv = document.createElement("div");
    rangeDiv.classList.add("range");
    volRangeDiv.appendChild(rangeDiv);

    let peakMeterDiv = document.createElement("div");
    peakMeterDiv.classList.add("peak-meter");
    rangeDiv.appendChild(peakMeterDiv);

    this.peakMeterBarDiv = document.createElement("div");
    this.peakMeterBarDiv.classList.add("peak-meter-bar");
    peakMeterDiv.appendChild(this.peakMeterBarDiv);

    this.volumeInput = document.createElement("input");
    this.volumeInput.type = "range";
    this.volumeInput.min = "0";
    this.volumeInput.max = "100";
    this.volumeInput.value = `${this.volumePercent}`;
    rangeDiv.appendChild(this.volumeInput);

    this.indicatorDiv = document.createElement("div");
    this.indicatorDiv.classList.add("indicator");
    this.indicatorDiv.innerText = this.volumePercent;
    volRangeDiv.appendChild(this.indicatorDiv);

    this.sessionBarDiv.appendChild(this.iconDiv);
    this.sessionBarDiv.appendChild(this.labelSpan);
    this.sessionBarDiv.appendChild(volRangeDiv);

    rowsDiv.appendChild(this.sessionBarDiv);
  }
}

/* <div class="session bar">
  <div class="icon">
    <img
      src="https://pbs.twimg.com/profile_images/949787136030539782/LnRrYf6e_400x400.jpg"
      alt=""
    />
  </div>
  <span class="label">The quick brown fox jumps</span>
  <div class="vol-range">
    <button>🔊</button>
    <div class="range">
      <div class="peak-meter">
        <div class="peak-meter-bar"></div>
      </div>
      <input type="range" />
    </div>
    <div class="indicator">0</div>
  </div>
</div>; */

let sessionElements = [];
function createSessionElements(sessions) {
  sessionElements = [];
  rowsDiv.innerHTML = "";

  for (let session of sessions) {
    sessionElements.push(new SessionElement(null, session));
  }
}
