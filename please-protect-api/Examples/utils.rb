require 'json'
require 'net/http'
require 'uri'

def json?(str)
  JSON.parse(str)
  true
rescue JSON::ParserError
  false
end

def load_env(envFile)
  # load_env.rb
  File.readlines(envFile, chomp: true).each do |line|
    next if line.strip.empty? || line.start_with?('#')

    key, value = line.split('=', 2)
    ENV[key] = value
  end
end

def make_request(method, apiName, data)
  host = ENV['API_HTTP_ENDPOINT']
  apiKey = ENV['API_KEY']

  uri = URI.parse("#{host}/#{apiName}")  

  # แปลง method เช่น "post" → "Net::HTTP::Post"
  klass_name = "Net::HTTP::#{method.to_s.capitalize}"
  request_class = Object.const_get(klass_name)

  request = request_class.new(uri.request_uri)
  request['Content-Type'] = 'application/json'
  
  if (!apiKey.nil?)
    request.basic_auth("api", apiKey)
    puts("===== Using API KEY =====")
  end

  if (!data.nil?)
    request.body = data.to_json
  end

  http = Net::HTTP.new(uri.host, uri.port)  
  http.use_ssl = (uri.scheme == "https")

  response = http.request(request)

  if (response.code != '200')
    puts("ERROR : Failed to send request with error [#{response}]")
    return
  end

  result = response.body
  if json?(result)
    result = JSON.parse(result)
  end

  return result
end

def upload_file_to_gcs(presigned_url, file_path, content_type)
  uri = URI.parse(presigned_url)

  http = Net::HTTP.new(uri.host, uri.port)
  http.use_ssl = (uri.scheme == "https")

  request = Net::HTTP::Put.new(uri.request_uri)
  request['Content-Type'] = content_type
  request['x-goog-meta-onix-is-temp-file'] = 'true'
  request.body = File.read(file_path, mode: "rb")   # อ่านเป็น binary

  response = http.request(request)

  if response.is_a?(Net::HTTPSuccess)
    puts "✅ Upload สำเร็จ: #{file_path}"
  else
    puts "❌ Upload ล้มเหลว: #{response.code} #{response.message}"
    puts response.body
  end
end
