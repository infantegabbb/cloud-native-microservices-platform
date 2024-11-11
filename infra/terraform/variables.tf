variable "namespace" {
  type        = string
  description = "Kubernetes namespace"
  default     = "portfolio-microservices"
}

variable "user_image" {
  type    = string
  default = "ghcr.io/jeffrey-dev0705/cloud-native-user-service:latest"
}

variable "order_image" {
  type    = string
  default = "ghcr.io/jeffrey-dev0705/cloud-native-order-service:latest"
}
