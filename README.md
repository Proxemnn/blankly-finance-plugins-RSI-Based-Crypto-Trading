<br />

<div align="center">
   <img style="margin: 0 auto; padding-bottom: 15px; padding-top: 30px" width=70%" src="https://firebasestorage.googleapis.com/v0/b/blankly-6ada5.appspot.com/o/blankly-github-logo.png?alt=media&token=8f436cd2-3d28-432c-867a-afef780f4260">
</div>
<br />

<div align="center">
  <b>💨  Rapidly build and deploy quantitative models for stocks, crypto, and forex  🚀</b>
</div>
<br />

[![Discord Shield](https://img.shields.io/discord/831165782750789672)](https://discord.com/invite/xPHTuHCmuV)
[![Open in Dev Containers](https://img.shields.io/static/v1?label=Dev%20Containers&message=Open&color=blue&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/OpenBB-finance/OpenBB)
<a href="https://codespaces.new/OpenBB-finance/OpenBB">
  <img src="https://github.com/codespaces/badge.svg" height="20" />
</a>
<a target="_blank" href="https://colab.research.google.com/github/OpenBB-finance/OpenBB/blob/develop/examples/googleColab.ipynb">
  <img src="https://colab.research.google.com/assets/colab-badge.svg" alt="Open In Colab"/>
</a>
[![PyPI](https://img.shields.io/pypi/v/openbb?color=blue&label=PyPI%20Package)](https://pypi.org/project/openbb/)


## Why Blankly Plugins?

Blankly Plugins let you **build once, use anywhere** across backtests, paper trading, and live markets.

They are:
- 🔌 Modular & reusable components
- 📦 Environment-agnostic (backtest, sandbox, live)
- 🧩 Easy to plug into any strategy or workflow
- 🌐 Exchange-agnostic with unified interfaces

Create, share, or combine plugins for indicators, strategies, risk controls, and more — all while keeping your code clean and scalable.

<div align="center">
<a target="_blank" href="https://youtu.be/pcm0h63rhUU"><img src="https://firebasestorage.googleapis.com/v0/b/blankly-6ada5.appspot.com/o/github%2Fbuild_a_bot_readme_thumbnail.jpg?alt=media&token=a9dd030a-805c-447f-a970-2bc8e1815662" style="border-radius:10px; width: 50%"></a>
</div>

### Trade Stocks, Crypto, Futures, and Forex

```python
from blankly import Alpaca, CoinbasePro

stocks = Alpaca()
crypto = CoinbasePro()
futures = BinanceFutures()

# Easily perform the same actions across exchanges & asset types
stocks.interface.market_order('AAPL', 'buy', 1)
crypto.interface.market_order('BTC-USD', 'buy', 1)
# Full futures feature set
futures.interface.get_hedge_mode()
```

### Backtest your trades, events, websockets, and custom data

```python
import blankly
"""
This example shows how backtest over tweets
"""

class TwitterBot(blankly.Model):
    def main(self, args):
        while self.has_data:
            self.backtester.value_account()
            self.sleep('1h')

    def event(self, type_: str, data: str):
        # Now check if it's a tweet about Tesla
        if 'tsla' in data.lower() or 'gme' in data.lower():
            # Buy, sell or evaluate your portfolio
            pass


if __name__ == "__main__":
    exchange = blankly.Alpaca()
    model = TwitterBot(exchange)

    # Add the tweets json here
    model.backtester.add_custom_events(blankly.data.JsonEventReader('./tweets.json'))
    # Now add some underlying prices at 1 month
    model.backtester.add_prices('TSLA', '1h', start_date='3/20/22', stop_date='4/15/22')

    # Backtest or run live
    print(model.backtest(args=None, initial_values={'USD': 10000}))

```

#### Accurate Backtest Holdings

<div align="center">
    <a><img src="https://firebasestorage.googleapis.com/v0/b/blankly-6ada5.appspot.com/o/github%2FScreen%20Shot%202022-04-17%20at%202.37.58%20PM.png?alt=media&token=d5738617-e197-4da2-850d-8fbbfda05275" style="border-radius:10px"></a>
</div>


## 🛠️ Installation

Follow the steps below to install and run this project on your local machine.

### 1. 📦 Prerequisites

Make sure you have the following tools installed on your system:

- [Node.js](https://nodejs.org/)  (v14.x or newer)
- [npm](https://www.npmjs.com/)  (comes with Node.js)
- [Git](https://git-scm.com/) 
- Python 3.x (optional, if any indicators or tools require it)

> You can also use Docker to run the project in an isolated environment.

### 3. 📦 Install Dependencies

Install the required npm packages:

```bash
npm install
```

If the plugin uses Python-based tools or indicators, install the Python dependencies as well:

```bash
pip install -r requirements.txt
```

---

### 4. 🔐 Set Up Exchange API Keys

To allow the plugin to interact with cryptocurrency exchanges, set up your API credentials.

Create a `.env` file in the root directory and add your exchange keys:

```env
EXCHANGE_API_KEY=your_api_key_here
EXCHANGE_SECRET_KEY=your_secret_key_here
```

> Make sure not to commit this file to version control. It should remain private.

---

### 5. ⚙️ Configure the Plugin

Each plugin comes with a configuration file. For example:

```yaml
# config/rsi-config.yaml
strategy:
  rsi_period: 14
  overbought_threshold: 70
  oversold_threshold: 30
  symbol: BTC/USDT
  interval: "1h"
```

You can modify these values based on your trading preferences.

---

### 6. ▶️ Run the Plugin

Once everything is set up, start the plugin:

```bash
npm start -- --plugin rsi --config config/rsi-config.yaml
```

Or, if you're using a custom script:

```bash
node index.js --plugin rsi --config config/rsi-config.yaml
```

---

### 7. 🐳 Optional: Run with Docker

Build and run the plugin using Docker:

```bash
docker build -t blankly-rsi-plugin .
docker run -it --env-file .env blankly-rsi-plugin
```

```mermaid
erDiagram
    PLUGIN {
        string id
        string name
        string type
    }

    PLUGIN ||--o{ STRATEGY : implements
    STRATEGY ||--o{ INDICATOR : uses
    STRATEGY ||--o{ EXECUTOR : runs
    EXECUTOR ||--o{ MARKET_INTERFACE : interacts
    MARKET_INTERFACE }|--o{ EXCHANGE : connects
    PLUGIN ||--o{ CONFIGURATION : requires
    PLUGIN ||--o{ LOGGING : logs
```



<p align="center">
    <img src="https://minkxx-spotify-readme.vercel.app/api?theme=dark&rainbow=true&scan=true&spin=True" alt="Preview">
</p>

<p align="center">
  <img src="https://github.com/tarikmanoar/tarikmanoar/raw/output/github-snake-dark.svg" alt="snake"></center>
</p>

## Contributors

 wouldn't be  without you. If we are going to disrupt financial industry, every contribution counts. Thank you for being part of this journey.

<a href="https://github.com/OpenBB-finance/OpenBB/graphs/contributors">
   <img src="https://contributors-img.web.app/image?repo=OpenBB-finance/OpenBB" width="800"/>
</a>
