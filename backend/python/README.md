# Python Backend

## Setting up a virtual environment

1. Run the following command to create a virtual environment

```
$ python -m venv env
```

2. Activate the virtual environment

```
$ source env/bin/activate
```

Once the environment is active you should see `(env)` prepended to your bash prompt similar
to below

```
(env) $
```

4. Install the necessary packages into the virtual environment

```
python -m pip install -r requirements.txt
```

## Running the Python backend 

`python worker.py` to start the worker program
