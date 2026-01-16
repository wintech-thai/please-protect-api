#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require "securerandom"
require './utils'
require "faraday"
require "faraday/multipart"
require "marcel"
require "pathname"

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

filePath = "egg-10-xx.png"
fileName = File.basename(filePath)
docType = "XFORM"

#### Get presigned URL ####
apiUrl = "api/Document/org/#{orgId}/action/GetUploadPresignedUrl"
param = {
  FileName: "#{fileName}",
  DocumentType: "#{docType}", #ตรงนี้รับมาจากหน้า form ให้เลือก
}
result = make_request(:post, apiUrl, param)

json = result.to_json
#puts(json)

#### Upload document
#### ต้องเช็คด้วยนะว่า status จาก GetUploadPresignedUrl ได้เป็น OK หรือไม่ แต่ในที่นี้จะไม่เช็คออะไร

bucket = result['presignedResult']['fields']['bucket']
storagePath = result['presignedResult']['fields']['key']
amzDate = result['presignedResult']['fields']['x-amz-date']
amzCredential = result['presignedResult']['fields']['x-amz-credential']
amzAlgorithm = result['presignedResult']['fields']['x-amz-algorithm']
uploadUrl = result['presignedResult']['url']
requiredFields = result['presignedResult']['fields']


mime = Marcel::MimeType.for(Pathname.new(filePath)) #แกะ MIME type อัตโนมัติให้เลย
puts("Uploading [#{mime}], [#{storagePath}], [#{uploadUrl}]...")

conn = Faraday.new do |f|
  f.request :multipart
end
response = conn.post(uploadUrl) do |req|
  req.body = requiredFields.merge(
    file: Faraday::UploadIO.new(filePath, mime || "application/octet-stream")
  )
end

status = response.status
if (status != 204)
  puts("Uploaded done with status [#{response.status}]")
  puts(response.body)

  exit 1
end

#### AddDocument ####
apiUrl = "api/Document/org/#{orgId}/action/AddDocument"
param = {
  DocName: "#{amzDate}:#{fileName}",
  Description: "#{filePath}", #ตรงนี้รับมาจากหน้า form ให้กรอก
  Tags: "testing3",
  DocType: "#{docType}",
  Bucket: "#{bucket}",
  Path: "#{storagePath}"
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
