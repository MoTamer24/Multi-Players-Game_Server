// المتغيرات العامة
let currentUser = "";
let currentRoom = "";

// --- 1. منطق التنقل (Navigation) ---
function showServers() {
    currentUser = document.getElementById('username').value.trim();
    if (currentUser === "") return alert("من فضلك أدخل اسمك");
    
    document.getElementById('loginScreen').style.display = 'none';
    document.getElementById('serverScreen').style.display = 'flex';
}

function createServer() {
    const sName = document.getElementById('serverName').value.trim();
    if (sName !== "") joinServer(sName);
}

function joinServer(serverName) {
    currentRoom = serverName;
    document.getElementById('serverScreen').style.display = 'none';
    document.getElementById('mainGame').style.display = 'grid';
    document.getElementById('currentRoomName').innerText = serverName;
    
    addChatMessage(`مرحباً ${currentUser}، انضممت إلى ${serverName}`, true);
}

// --- 2. منطق المحادثة ---
const msgInput = document.getElementById('msgInput');
const sendBtn = document.getElementById('sendBtn');
const chatMessages = document.getElementById('chatMessages');

function addChatMessage(text, isSystem = false) {
    const msgDiv = document.createElement('div');
    msgDiv.className = isSystem ? 'message system' : 'message';
    msgDiv.innerText = isSystem ? text : `${currentUser}: ${text}`;
    chatMessages.appendChild(msgDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

sendBtn.onclick = () => {
    if (msgInput.value.trim() !== "") {
        addChatMessage(msgInput.value);
        msgInput.value = "";
    }
};

// --- 3. منطق لعبة XO ---
const cells = document.querySelectorAll('.cell');
const statusText = document.getElementById('status');
const resetBtn = document.getElementById('resetBtn');
let currentPlayer = "X";
let boardState = ["", "", "", "", "", "", "", "", ""];
let isGameActive = true;

const winPatterns = [
    [0,1,2], [3,4,5], [6,7,8], [0,3,6], [1,4,7], [2,5,8], [0,4,8], [2,4,6]
];

cells.forEach(cell => {
    cell.onclick = (e) => {
        const idx = e.target.getAttribute('data-index');
        if (boardState[idx] !== "" || !isGameActive) return;

        boardState[idx] = currentPlayer;
        e.target.innerText = currentPlayer;
        e.target.style.color = currentPlayer === "X" ? "#00d2ff" : "#9d50bb";
        
        checkWinner();
    };
});

function checkWinner() {
    let won = false;
    for (let p of winPatterns) {
        if (boardState[p[0]] && boardState[p[0]] === boardState[p[1]] && boardState[p[0]] === boardState[p[2]]) {
            won = true; break;
        }
    }

    if (won) {
        statusText.innerText = `انتهت! الفائز هو ${currentPlayer}`;
        isGameActive = false;
    } else if (!boardState.includes("")) {
        statusText.innerText = "تعادل!";
        isGameActive = false;
    } else {
        currentPlayer = currentPlayer === "X" ? "O" : "X";
        statusText.innerText = `دور اللاعب: ${currentPlayer}`;
    }
}

resetBtn.onclick = () => {
    boardState = ["", "", "", "", "", "", "", "", ""];
    isGameActive = true;
    currentPlayer = "X";
    statusText.innerText = "دور اللاعب: X";
    cells.forEach(c => c.innerText = "");
};