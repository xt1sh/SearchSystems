from flask import Flask, request, jsonify
from sentence_transformers import SentenceTransformer

app = Flask(__name__)

print('Server started')

# Choose the trained model for the sentence transformer
model = SentenceTransformer('all-MiniLM-L6-v2')

@app.route('/vectorize', methods=['GET'])
def vectorize():
    """
    This endpoint takes a string in the request query and returns embeddings for this string.
    """
    query = request.args.get('query')
    if query:
        embeddings = model.encode(query)
        return jsonify({'embeddings': embeddings.tolist()})
    else:
        return jsonify({'error': 'Query parameter "query" is missing.'}), 400
    
@app.route('/vectorizeBulk', methods=['POST'])
def vectorize_bulk():
    """
    This endpoint takes a list of strings in the request body and returns a JSON array with embeddings for each string.
    """
    data = request.get_json()
    if not data or not isinstance(data, list):
        return jsonify({'error': 'Invalid request body. Please provide a list of strings.'}), 400

    embeddings = model.encode(data)
    results = []
    for i, embedding in enumerate(embeddings):
        results.append({"initialString": data[i], "embeddings": embedding.tolist()})

    return jsonify(results)

@app.route('/')
def index():
    return 'OK'

if __name__ == '__main__':
    from waitress import serve
    serve(app, host="0.0.0.0", port=1057)